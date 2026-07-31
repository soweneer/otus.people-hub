using StackExchange.Redis;

namespace PeopleHub.Counters.Storage;

internal sealed class RedisCounterStore(IConnectionMultiplexer redis) : ICounterStore
{
    private const string TotalField = "total";

    private const string ApplyMessageScript =
        """
        local watermark = tonumber(redis.call('HGET', KEYS[2], ARGV[1]) or '0')
        local messageId = tonumber(ARGV[2])

        if messageId <= watermark then
          return tonumber(redis.call('HGET', KEYS[1], 'total') or '0')
        end

        if redis.call('ZADD', KEYS[3], messageId, messageId) == 0 then
          return tonumber(redis.call('HGET', KEYS[1], 'total') or '0')
        end

        redis.call('HSET', KEYS[1], ARGV[1], redis.call('ZCARD', KEYS[3]))

        return redis.call('HINCRBY', KEYS[1], 'total', 1)
        """;

    private const string ApplyReadScript =
        """
        local upTo = tonumber(ARGV[2])
        local watermark = tonumber(redis.call('HGET', KEYS[2], ARGV[1]) or '0')

        if upTo > watermark then
          redis.call('HSET', KEYS[2], ARGV[1], upTo)
        end

        local removed = redis.call('ZREMRANGEBYSCORE', KEYS[3], '-inf', upTo)
        if removed == 0 then
          return tonumber(redis.call('HGET', KEYS[1], 'total') or '0')
        end

        local left = redis.call('ZCARD', KEYS[3])
        if left == 0 then
          redis.call('HDEL', KEYS[1], ARGV[1])
        else
          redis.call('HSET', KEYS[1], ARGV[1], left)
        end

        return redis.call('HINCRBY', KEYS[1], 'total', -removed)
        """;

    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<long> ApplyMessageAsync(long userId, long partnerId, long messageId,
        CancellationToken cancellationToken = default)
    {
        var result = await _database.ScriptEvaluateAsync(ApplyMessageScript,
            [CountsKey(userId), ReadsKey(userId), PendingKey(userId, partnerId)],
            [partnerId, messageId]);

        return (long)result;
    }

    public async Task<long> ApplyReadAsync(long userId, long partnerId, long upToMessageId,
        CancellationToken cancellationToken = default)
    {
        var result = await _database.ScriptEvaluateAsync(ApplyReadScript,
            [CountsKey(userId), ReadsKey(userId), PendingKey(userId, partnerId)],
            [partnerId, upToMessageId]);

        return (long)result;
    }

    public async Task<CounterSnapshot> GetAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entries = await _database.HashGetAllAsync(CountsKey(userId));

        var counters = new List<PartnerCount>(entries.Length);
        var total = 0L;

        foreach (var entry in entries)
        {
            if (entry.Name == TotalField)
            {
                total = (long)entry.Value;
                continue;
            }

            counters.Add(new PartnerCount((long)entry.Name, (long)entry.Value));
        }

        return new CounterSnapshot(counters, total);
    }

    public async Task<long> GetTotalAsync(long userId, CancellationToken cancellationToken = default)
    {
        var total = await _database.HashGetAsync(CountsKey(userId), TotalField);

        return total.IsNull ? 0L : (long)total;
    }

    private static string Tag(long userId) => "unread:{" + userId + "}";

    private static RedisKey CountsKey(long userId) => Tag(userId);

    private static RedisKey ReadsKey(long userId) => Tag(userId) + ":read";

    private static RedisKey PendingKey(long userId, long partnerId) => Tag(userId) + ":" + partnerId;
}
