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

         create table if not exists {DbClient.DialogReadsTable} (
             dialog_id bigint not null,
             user_id bigint not null,
             last_read_message_id bigint not null default 0,
             updated_at timestamptz not null default now(),
             primary key (dialog_id, user_id)
         );

         create table if not exists {DbClient.OutboxTable} (
             id bigint generated always as identity primary key,
             aggregate_id bigint not null,
             type text not null,
             payload jsonb not null,
             created_at timestamptz not null default now(),
             published_at timestamptz
         );

         create index if not exists ix_{DbClient.OutboxTable}_pending on {DbClient.OutboxTable} (id) where published_at is null;
         """;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await dbClient.ExecuteNonQueryAsync(CreateTablesSql, cancellationToken: cancellationToken);
    }
}
