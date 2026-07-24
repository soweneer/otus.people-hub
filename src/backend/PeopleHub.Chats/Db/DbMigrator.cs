using Microsoft.Extensions.Options;

namespace PeopleHub.Chats.Db;

internal sealed class DbMigrator(DbClient dbClient, IOptions<CitusOptions> citusOptions) : IDbMigrator
{
    private const string CreateTablesSql =
        $"""
         create table if not exists {DbClient.DialogsTable} (
             id bigint generated always as identity primary key,
             user_id1 bigint not null,
             user_id2 bigint not null,
             unique (user_id1, user_id2)
         );

         create table if not exists {DbClient.MessagesTable} (
             id bigint generated always as identity,
             dialog_id bigint not null,
             from_user_id bigint not null,
             text text not null,
             created_at timestamptz not null default now(),
             primary key (dialog_id, id)
         );

         create index if not exists ix_{DbClient.MessagesTable}_dialog on {DbClient.MessagesTable} (dialog_id, id);
         """;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        var citus = citusOptions.Value;

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

        await dbClient.ExecuteNonQueryAsync(CreateTablesSql, cancellationToken: cancellationToken);

        await dbClient.ExecuteNonQueryAsync(
            $"select create_reference_table('{DbClient.DialogsTable}') " +
            $"where not exists (select 1 from pg_dist_partition where logicalrelid = '{DbClient.DialogsTable}'::regclass);",
            cancellationToken: cancellationToken);

        await dbClient.ExecuteNonQueryAsync(
            $"select create_distributed_table('{DbClient.MessagesTable}', 'dialog_id', shard_count => {citus.ShardCount}) " +
            $"where not exists (select 1 from pg_dist_partition where logicalrelid = '{DbClient.MessagesTable}'::regclass);",
            cancellationToken: cancellationToken);
    }
}
