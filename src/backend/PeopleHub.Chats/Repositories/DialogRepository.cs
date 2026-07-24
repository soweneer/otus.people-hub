using System.Data;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Repositories;

internal sealed class DialogRepository(DbClient dbClient) : IDialogRepository
{
    public async Task<long> GetOrCreateDialogIdAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        var (first, second) = Normalize(userId1, userId2);

        const string query =
            $"""
             insert into {DbClient.DialogsTable} (user_id1, user_id2)
             values (@userId1, @userId2)
             on conflict (user_id1, user_id2) do update set user_id1 = {DbClient.DialogsTable}.user_id1
             returning id
             """;

        var id = await dbClient.ExecuteScalarAsync(query,
            [("userId1", first), ("userId2", second)],
            cancellationToken);

        return Convert.ToInt64(id);
    }

    public async Task<long?> GetDialogIdAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        var (first, second) = Normalize(userId1, userId2);

        const string query =
            $"select id from {DbClient.DialogsTable} where user_id1 = @userId1 and user_id2 = @userId2";

        var id = await dbClient.ExecuteScalarAsync(query,
            [("userId1", first), ("userId2", second)],
            cancellationToken);

        return id is null or DBNull ? null : Convert.ToInt64(id);
    }

    public async Task<long> AddMessageAsync(DialogMessage message, CancellationToken cancellationToken = default)
    {
        const string query =
            $"insert into {DbClient.MessagesTable} (dialog_id, from_user_id, text) " +
            "values (@dialogId, @fromUserId, @text) returning id";

        var id = await dbClient.ExecuteScalarAsync(query,
            [
                ("dialogId", message.DialogId),
                ("fromUserId", message.FromUserId),
                ("text", message.Text)
            ],
            cancellationToken);

        return Convert.ToInt64(id);
    }

    public async Task<IReadOnlyCollection<DialogMessage>> GetMessagesAsync(long dialogId, CancellationToken cancellationToken = default)
    {
        const string query =
            $"select id, dialog_id, from_user_id, text from {DbClient.MessagesTable} " +
            "where dialog_id = @dialogId " +
            "order by id";

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("dialogId", dialogId)],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>().Select(ExtractMessage).ToArray();
    }

    public async Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string query =
            $"""
             select case when user_id1 = @userId then user_id2 else user_id1 end as partner_id
             from {DbClient.DialogsTable}
             where user_id1 = @userId or user_id2 = @userId
             order by partner_id
             """;

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("userId", userId)],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>().Select(row => Convert.ToInt64(row["partner_id"])).ToArray();
    }

    private static (long, long) Normalize(long userId1, long userId2) =>
        userId1 <= userId2 ? (userId1, userId2) : (userId2, userId1);

    private static DialogMessage ExtractMessage(DataRow row) =>
        DialogMessage.Restore(
            Convert.ToInt64(row["id"]),
            Convert.ToInt64(row["dialog_id"]),
            Convert.ToInt64(row["from_user_id"]),
            row["text"].ToString());
}
