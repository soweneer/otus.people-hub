using System.Data;
using System.Text.Json;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Outbox;

namespace PeopleHub.Chats.Repositories;

internal sealed class DialogRepository(DbClient dbClient) : IDialogRepository
{
    private const string InsertOutboxSql =
        $"""
         insert into {DbClient.OutboxTable} (aggregate_id, type, payload)
         values (@aggregateId, @type, @payload::jsonb)
         """;

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

    public Task<SentMessage> AddMessageAsync(long fromUserId, long toUserId, DialogMessage message,
        CancellationToken cancellationToken = default) =>
        dbClient.InTransactionAsync(async scope =>
        {
            var dialogId = await GetOrCreateDialogIdAsync(scope, fromUserId, toUserId, cancellationToken);

            const string insertMessage =
                $"insert into {DbClient.MessagesTable} (dialog_id, from_user_id, text) " +
                "values (@dialogId, @fromUserId, @text) returning id";

            var messageId = Convert.ToInt64(await scope.ExecuteScalarAsync(insertMessage,
                [
                    ("dialogId", dialogId),
                    ("fromUserId", fromUserId),
                    ("text", message.Text)
                ],
                cancellationToken));

            var payload = new MessageSentPayload(messageId, dialogId, fromUserId, toUserId);
            await scope.ExecuteNonQueryAsync(InsertOutboxSql,
                [
                    ("aggregateId", dialogId),
                    ("type", OutboxEventTypes.MessageSent),
                    ("payload", JsonSerializer.Serialize(payload, JsonSerializerOptions.Web))
                ],
                cancellationToken);

            return new SentMessage(messageId, dialogId);
        }, cancellationToken);

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

    public Task<long> MarkReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default) =>
        dbClient.InTransactionAsync(async scope =>
        {
            var dialogId = await FindDialogIdAsync(scope, userId, partnerId, cancellationToken);
            if (dialogId is null)
            {
                return 0L;
            }

            const string upsert =
                $"""
                 insert into {DbClient.DialogReadsTable} as target (dialog_id, user_id, last_read_message_id)
                 values (
                     @dialogId,
                     @userId,
                     case when @upTo > 0
                         then @upTo
                         else coalesce((select max(id) from {DbClient.MessagesTable} where dialog_id = @dialogId), 0)
                     end
                 )
                 on conflict (dialog_id, user_id) do update
                     set last_read_message_id = excluded.last_read_message_id,
                         updated_at = now()
                     where target.last_read_message_id < excluded.last_read_message_id
                 returning last_read_message_id
                 """;

            var advanced = await scope.ExecuteScalarAsync(upsert,
                [("dialogId", dialogId.Value), ("userId", userId), ("upTo", upToMessageId)],
                cancellationToken);

            if (advanced is null or DBNull)
            {
                return await ReadWatermarkAsync(scope, dialogId.Value, userId, cancellationToken);
            }

            var lastReadMessageId = Convert.ToInt64(advanced);
            var payload = new DialogReadPayload(dialogId.Value, userId, partnerId, lastReadMessageId);

            await scope.ExecuteNonQueryAsync(InsertOutboxSql,
                [
                    ("aggregateId", dialogId.Value),
                    ("type", OutboxEventTypes.DialogRead),
                    ("payload", JsonSerializer.Serialize(payload, JsonSerializerOptions.Web))
                ],
                cancellationToken);

            return lastReadMessageId;
        }, cancellationToken);

    public async Task<IReadOnlyCollection<UnreadCount>> GetUnreadCountsAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string query =
            $"""
             select case when d.user_id1 = @userId then d.user_id2 else d.user_id1 end as partner_id,
                    count(m.id) as unread
             from {DbClient.DialogsTable} d
             join {DbClient.MessagesTable} m on m.dialog_id = d.id and m.from_user_id <> @userId
             left join {DbClient.DialogReadsTable} r on r.dialog_id = d.id and r.user_id = @userId
             where (d.user_id1 = @userId or d.user_id2 = @userId)
               and m.id > coalesce(r.last_read_message_id, 0)
             group by partner_id
             order by partner_id
             """;

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("userId", userId)],
            cancellationToken);

        return dataTable is null
            ? []
            : dataTable.Rows.Cast<DataRow>()
                .Select(row => new UnreadCount(Convert.ToInt64(row["partner_id"]), Convert.ToInt64(row["unread"])))
                .ToArray();
    }

    public async Task<IReadOnlyCollection<UnreadPartnerState>> GetUnreadStateAsync(long userId, int limitPerPartner,
        CancellationToken cancellationToken = default)
    {
        const string query =
            $"""
             select partner_id, last_read, message_id
             from (
                 select case when d.user_id1 = @userId then d.user_id2 else d.user_id1 end as partner_id,
                        coalesce(r.last_read_message_id, 0) as last_read,
                        m.id as message_id,
                        row_number() over (
                            partition by case when d.user_id1 = @userId then d.user_id2 else d.user_id1 end
                            order by m.id desc
                        ) as position
                 from {DbClient.DialogsTable} d
                 join {DbClient.MessagesTable} m on m.dialog_id = d.id and m.from_user_id <> @userId
                 left join {DbClient.DialogReadsTable} r on r.dialog_id = d.id and r.user_id = @userId
                 where (d.user_id1 = @userId or d.user_id2 = @userId)
                   and m.id > coalesce(r.last_read_message_id, 0)
             ) ranked
             where position <= @limit
             order by partner_id, message_id
             """;

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [("userId", userId), ("limit", limitPerPartner)],
            cancellationToken);

        if (dataTable is null)
        {
            return [];
        }

        return dataTable.Rows.Cast<DataRow>()
            .GroupBy(row => (Convert.ToInt64(row["partner_id"]), Convert.ToInt64(row["last_read"])))
            .Select(group => new UnreadPartnerState(
                group.Key.Item1,
                group.Key.Item2,
                group.Select(row => Convert.ToInt64(row["message_id"])).ToArray()))
            .ToArray();
    }

    private static async Task<long> GetOrCreateDialogIdAsync(DbTransactionScope scope, long userId1, long userId2,
        CancellationToken cancellationToken)
    {
        var (first, second) = Normalize(userId1, userId2);

        const string query =
            $"""
             insert into {DbClient.DialogsTable} (user_id1, user_id2)
             values (@userId1, @userId2)
             on conflict (user_id1, user_id2) do update set user_id1 = {DbClient.DialogsTable}.user_id1
             returning id
             """;

        var id = await scope.ExecuteScalarAsync(query,
            [("userId1", first), ("userId2", second)],
            cancellationToken);

        return Convert.ToInt64(id);
    }

    private static async Task<long?> FindDialogIdAsync(DbTransactionScope scope, long userId1, long userId2,
        CancellationToken cancellationToken)
    {
        var (first, second) = Normalize(userId1, userId2);

        const string query =
            $"select id from {DbClient.DialogsTable} where user_id1 = @userId1 and user_id2 = @userId2";

        var id = await scope.ExecuteScalarAsync(query,
            [("userId1", first), ("userId2", second)],
            cancellationToken);

        return id is null or DBNull ? null : Convert.ToInt64(id);
    }

    private static async Task<long> ReadWatermarkAsync(DbTransactionScope scope, long dialogId, long userId,
        CancellationToken cancellationToken)
    {
        const string query =
            $"select last_read_message_id from {DbClient.DialogReadsTable} where dialog_id = @dialogId and user_id = @userId";

        var value = await scope.ExecuteScalarAsync(query,
            [("dialogId", dialogId), ("userId", userId)],
            cancellationToken);

        return value is null or DBNull ? 0L : Convert.ToInt64(value);
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
