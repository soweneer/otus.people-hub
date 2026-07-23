using System.Data;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Repositories;

internal sealed class DialogRepository(DbClient dbClient) : IDialogRepository
{
    public Task AddAsync(DialogMessage message, CancellationToken cancellationToken = default)
    {
        const string query =
            $"insert into {DbClient.DialogsTable} (chat_key, id, from_user_id, to_user_id, text) " +
            "values (@chatKey, @id, @fromUserId, @toUserId, @text)";

        return dbClient.ExecuteNonQueryAsync(query,
            [
                ("chatKey", ChatKey(message.FromUserId, message.ToUserId)),
                ("id", message.Id),
                ("fromUserId", message.FromUserId),
                ("toUserId", message.ToUserId),
                ("text", message.Text)
            ],
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        const string query =
            $"select id, from_user_id, to_user_id, text from {DbClient.DialogsTable} " +
            "where chat_key = @chatKey " +
            "order by id";

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("chatKey", ChatKey(userId1, userId2))],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>().Select(ExtractMessage).ToArray();
    }

    public async Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string query =
            $"""
             select case when from_user_id = @userId then to_user_id else from_user_id end as partner_id,
                    max(created_at) as last_at
             from {DbClient.DialogsTable}
             where from_user_id = @userId or to_user_id = @userId
             group by 1
             order by last_at desc
             """;

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("userId", userId)],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>().Select(row => Convert.ToInt64(row["partner_id"])).ToArray();
    }

    private static string ChatKey(long userId1, long userId2) =>
        userId1 <= userId2 ? $"{userId1}:{userId2}" : $"{userId2}:{userId1}";

    private static DialogMessage ExtractMessage(DataRow row) =>
        DialogMessage.Restore(
            (Guid)row["id"],
            Convert.ToInt64(row["from_user_id"]),
            Convert.ToInt64(row["to_user_id"]),
            row["text"].ToString());
}
