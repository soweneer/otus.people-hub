namespace PeopleHub.Chats.Db;

internal sealed class DbMigrator(DbClient dbClient) : IDbMigrator
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
        await dbClient.ExecuteNonQueryAsync(CreateTablesSql, cancellationToken: cancellationToken);
    }
}
