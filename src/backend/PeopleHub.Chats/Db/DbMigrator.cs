namespace PeopleHub.Chats.Db;

internal sealed class DbMigrator(DbClient dbClient) : IDbMigrator
{
    private const string CreateSql =
        $"""
         create table if not exists {DbClient.DialogsTable} (
             id bigint generated always as identity primary key,
             from_user_id integer not null,
             to_user_id integer not null,
             text text not null,
             created_at timestamptz not null default now()
         );

         create index if not exists ix_{DbClient.DialogsTable}_from_to on {DbClient.DialogsTable} (from_user_id, to_user_id, id);
         create index if not exists ix_{DbClient.DialogsTable}_to_from on {DbClient.DialogsTable} (to_user_id, from_user_id, id);
         """;

    public Task MigrateAsync(CancellationToken cancellationToken = default) =>
        dbClient.ExecuteNonQueryAsync(CreateSql, cancellationToken);
}
