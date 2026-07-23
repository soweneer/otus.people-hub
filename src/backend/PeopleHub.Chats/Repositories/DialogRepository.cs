using System.Data;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Repositories;

internal sealed class DialogRepository(DbClient dbClient) : IDialogRepository
{
    public async Task<long?> AddAsync(DialogMessage message, CancellationToken cancellationToken = default)
    {
        const string query =
            $"insert into {DbClient.DialogsTable} (from_user_id, to_user_id, text) " +
            "values (@fromUserId, @toUserId, @text) returning id";

        var id = await dbClient.ExecuteScalarAsync(query,
            [
                ("fromUserId", message.FromUserId),
                ("toUserId", message.ToUserId),
                ("text", message.Text)
            ],
            cancellationToken);

        return id is null or DBNull ? null : Convert.ToInt64(id);
    }

    public async Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        const string query =
            $"select id, from_user_id, to_user_id, text from {DbClient.DialogsTable} " +
            "where (from_user_id = @userId1 and to_user_id = @userId2) " +
            "   or (from_user_id = @userId2 and to_user_id = @userId1) " +
            "order by id";

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [
                ("userId1", userId1),
                ("userId2", userId2)
            ],
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
                    max(id) as last_id
             from {DbClient.DialogsTable}
             where from_user_id = @userId or to_user_id = @userId
             group by 1
             order by last_id desc
             """;

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("userId", userId)],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>().Select(row => Convert.ToInt64(row["partner_id"])).ToArray();
    }

    private static DialogMessage ExtractMessage(DataRow row) =>
        DialogMessage.Restore(
            Convert.ToInt64(row["id"]),
            Convert.ToInt64(row["from_user_id"]),
            Convert.ToInt64(row["to_user_id"]),
            row["text"].ToString());
}
