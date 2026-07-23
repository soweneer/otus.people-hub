using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class DialogRepository(DbClient dbClient) : IDialogRepository
{
    public async Task<long?> AddAsync(DialogMessage message, CancellationToken cancellationToken = default)
    {
        const string query =
            $"INSERT INTO {DbClient.DialogsTable} (from_user_id, to_user_id, text) " +
            "VALUES (@fromUserId, @toUserId, @text) RETURNING id";

        var id = await dbClient.ExecuteScalarAsync(query,
            [
                ("fromUserId", message.FromUserId),
                ("toUserId", message.ToUserId),
                ("text", message.Text)
            ]);

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

    private static DialogMessage ExtractMessage(DataRow row) =>
        DialogMessage.Restore(
            Convert.ToInt64(row["id"]),
            Convert.ToInt64(row["from_user_id"]),
            Convert.ToInt64(row["to_user_id"]),
            row["text"].ToString());
}
