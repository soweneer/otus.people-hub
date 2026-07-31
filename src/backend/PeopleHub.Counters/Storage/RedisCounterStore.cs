using StackExchange.Redis;

namespace PeopleHub.Counters.Storage;

internal sealed class RedisCounterStore(IConnectionMultiplexer redis) : ICounterStore
{
    private const string TotalField = "total";
    private const string SyncedField = "synced";
    private const string KnownUsersKey = "unread:users";

    private const string ApplyMessageScript =
        """
        local watermark = tonumber(redis.call('HGET', KEYS[2], ARGV[1]) or '0')
        local messageId = tonumber(ARGV[2])

        if messageId <= watermark or redis.call('ZADD', KEYS[3], messageId, messageId) == 0 then
          return {
            tonumber(redis.call('HGET', KEYS[1], ARGV[1]) or '0'),
            tonumber(redis.call('HGET', KEYS[1], 'total') or '0')
          }
        end

        local count = redis.call('ZCARD', KEYS[3])
        redis.call('HSET', KEYS[1], ARGV[1], count)

        return { count, redis.call('HINCRBY', KEYS[1], 'total', 1) }
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
          return {
            tonumber(redis.call('HGET', KEYS[1], ARGV[1]) or '0'),
            tonumber(redis.call('HGET', KEYS[1], 'total') or '0')
          }
        end

        local left = redis.call('ZCARD', KEYS[3])
        if left == 0 then
          redis.call('HDEL', KEYS[1], ARGV[1])
        else
          redis.call('HSET', KEYS[1], ARGV[1], left)
        end

        return { left, redis.call('HINCRBY', KEYS[1], 'total', -removed) }
        """;

    private const string ResetScript =
        """
        redis.call('DEL', KEYS[1], KEYS[2])
        for i = 3, #KEYS do
          redis.call('DEL', KEYS[i])
        end

        local partners = tonumber(ARGV[1])
        local total = 0
        local a = 2

        for i = 1, partners do
          local partner = ARGV[a]
          local lastRead = ARGV[a + 1]
          local count = tonumber(ARGV[a + 2])
          a = a + 3

          redis.call('HSET', KEYS[2], partner, lastRead)

          for j = 1, count do
            redis.call('ZADD', KEYS[2 + i], ARGV[a], ARGV[a])
            a = a + 1
          end

          if count > 0 then
            redis.call('HSET', KEYS[1], partner, count)
            total = total + count
          end
        end

        redis.call('HSET', KEYS[1], 'total', total)
        redis.call('HSET', KEYS[1], 'synced', ARGV[a])

        return total
        """;

    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<CounterState> ApplyMessageAsync(long userId, long partnerId, long messageId,
        CancellationToken cancellationToken = default)
    {
        await RememberUserAsync(userId);

        var result = await _database.ScriptEvaluateAsync(ApplyMessageScript,
            [CountsKey(userId), ReadsKey(userId), PendingKey(userId, partnerId)],
            [partnerId, messageId]);

        return ToState(result);
    }

    public async Task<CounterState> ApplyReadAsync(long userId, long partnerId, long upToMessageId,
        CancellationToken cancellationToken = default)
    {
        await RememberUserAsync(userId);

        var result = await _database.ScriptEvaluateAsync(ApplyReadScript,
            [CountsKey(userId), ReadsKey(userId), PendingKey(userId, partnerId)],
            [partnerId, upToMessageId]);

        return ToState(result);
    }

    public async Task<CounterSnapshot> GetAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entries = await _database.HashGetAllAsync(CountsKey(userId));
        if (!entries.Any(entry => entry.Name == SyncedField))
        {
            return null;
        }

        var counters = new List<PartnerCount>(entries.Length);
        var total = 0L;

        foreach (var entry in entries)
        {
            if (entry.Name == TotalField)
            {
                total = (long)entry.Value;
                continue;
            }

            if (entry.Name == SyncedField)
            {
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

    public async Task<long> ResetAsync(long userId, IReadOnlyCollection<PartnerUnreadState> partners,
        CancellationToken cancellationToken = default)
    {
        var known = await _database.HashKeysAsync(ReadsKey(userId));
        var stale = known
            .Select(field => (long)field)
            .Where(partnerId => partners.All(partner => partner.PartnerId != partnerId))
            .ToArray();

        var keys = new List<RedisKey>(partners.Count + stale.Length + 2)
        {
            CountsKey(userId),
            ReadsKey(userId)
        };
        keys.AddRange(partners.Select(partner => PendingKey(userId, partner.PartnerId)));
        keys.AddRange(stale.Select(partnerId => PendingKey(userId, partnerId)));

        var values = new List<RedisValue> { partners.Count };
        foreach (var partner in partners)
        {
            values.Add(partner.PartnerId);
            values.Add(partner.LastReadMessageId);
            values.Add(partner.UnreadMessageIds.Count);
            values.AddRange(partner.UnreadMessageIds.Select(id => (RedisValue)id));
        }

        values.Add(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        await RememberUserAsync(userId);
        var total = await _database.ScriptEvaluateAsync(ResetScript, keys.ToArray(), values.ToArray());

        return (long)total;
    }

    public async Task<IReadOnlyCollection<long>> GetKnownUserIdsAsync(CancellationToken cancellationToken = default)
    {
        var members = await _database.SetMembersAsync(KnownUsersKey);

        return members.Select(member => (long)member).ToArray();
    }

    private Task RememberUserAsync(long userId) =>
        _database.SetAddAsync(KnownUsersKey, userId, CommandFlags.FireAndForget);

    private static CounterState ToState(RedisResult result)
    {
        var values = (RedisValue[])result;

        return values is { Length: 2 }
            ? new CounterState((long)values[0], (long)values[1])
            : new CounterState(0, 0);
    }

    private static string Tag(long userId) => "unread:{" + userId + "}";

    private static RedisKey CountsKey(long userId) => Tag(userId);

    private static RedisKey ReadsKey(long userId) => Tag(userId) + ":read";

    private static RedisKey PendingKey(long userId, long partnerId) => Tag(userId) + ":" + partnerId;
}
