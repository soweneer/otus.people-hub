using System.Data;
using PeopleHub.Chats.Db;

namespace PeopleHub.Chats.Outbox;

internal sealed class OutboxDispatcher(DbClient dbClient)
{
    private const string SelectPendingSql =
        $"""
         select id, type, payload::text as payload
         from {DbClient.OutboxTable}
         where published_at is null
         order by id
         limit @batchSize
         for update skip locked
         """;

    private const string MarkPublishedSql =
        $"update {DbClient.OutboxTable} set published_at = now() where id = any(@ids)";

    private const string PendingStatsSql =
        $"""
         select count(*) as pending,
                coalesce(extract(epoch from now() - min(created_at)), 0) as lag_seconds
         from {DbClient.OutboxTable}
         where published_at is null
         """;

    public async Task<(long Pending, double LagSeconds)> GetPendingStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await dbClient.ExecuteDataTableAsync(PendingStatsSql, cancellationToken: cancellationToken);
        if (stats is null || stats.Rows.Count == 0)
        {
            return (0, 0);
        }

        var row = stats.Rows[0];

        return (Convert.ToInt64(row["pending"]), Convert.ToDouble(row["lag_seconds"]));
    }

    public Task<int> DrainAsync(Func<OutboxRecord, CancellationToken, Task> publish, int batchSize,
        CancellationToken cancellationToken = default) =>
        dbClient.InTransactionAsync(async scope =>
        {
            var pending = await scope.ExecuteDataTableAsync(SelectPendingSql,
                [("batchSize", batchSize)],
                cancellationToken);

            if (pending is null || pending.Rows.Count == 0)
            {
                return 0;
            }

            var records = pending.Rows.Cast<DataRow>()
                .Select(row => new OutboxRecord(
                    Convert.ToInt64(row["id"]),
                    row["type"].ToString(),
                    row["payload"].ToString()))
                .ToArray();

            foreach (var record in records)
            {
                await publish(record, cancellationToken);
            }

            await scope.ExecuteNonQueryAsync(MarkPublishedSql,
                [("ids", records.Select(record => record.Id).ToArray())],
                cancellationToken);

            return records.Length;
        }, cancellationToken);
}
