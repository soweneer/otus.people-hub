using Microsoft.Extensions.Options;

namespace PeopleHub.Chats.Db;

internal sealed class DbMigrator(DbClient dbClient, IOptions<CitusOptions> citusOptions) : IDbMigrator
{
    private const string CreateTableSql =
        $"""
         create table if not exists {DbClient.DialogsTable} (
             chat_key text not null,
             id uuid not null,
             from_user_id integer not null,
             to_user_id integer not null,
             text text not null,
             created_at timestamptz not null default now(),
             primary key (chat_key, id)
         );

         create index if not exists ix_{DbClient.DialogsTable}_from on {DbClient.DialogsTable} (from_user_id, id);
         create index if not exists ix_{DbClient.DialogsTable}_to on {DbClient.DialogsTable} (to_user_id, id);
         """;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        var citus = citusOptions.Value;

        if (citus.Enabled)
        {
            await dbClient.ExecuteNonQueryAsync("create extension if not exists citus;", cancellationToken: cancellationToken);

            await dbClient.ExecuteNonQueryAsync(
                $"select citus_set_coordinator_host('{citus.CoordinatorHost}', {citus.CoordinatorPort});",
                cancellationToken: cancellationToken);

            foreach (var worker in citus.Workers)
            {
                await dbClient.ExecuteNonQueryAsync(
                    $"select citus_add_node('{worker.Host}', {worker.Port}) " +
                    $"where not exists (select 1 from pg_dist_node where nodename = '{worker.Host}' and nodeport = {worker.Port});",
                    cancellationToken: cancellationToken);
            }
        }

        await dbClient.ExecuteNonQueryAsync(CreateTableSql, cancellationToken: cancellationToken);

        if (citus.Enabled)
        {
            await dbClient.ExecuteNonQueryAsync(
                $"select create_distributed_table('{DbClient.DialogsTable}', 'chat_key', shard_count => {citus.ShardCount}) " +
                $"where not exists (select 1 from pg_dist_partition where logicalrelid = '{DbClient.DialogsTable}'::regclass);",
                cancellationToken: cancellationToken);
        }
    }
}
