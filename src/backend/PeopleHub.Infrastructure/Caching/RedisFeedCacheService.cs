using System.Text.Json;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using StackExchange.Redis;

namespace PeopleHub.Infrastructure.Caching;

public class RedisFeedCacheService(IConnectionMultiplexer redis) : IFeedCacheService
{
    private const string KeyPrefix = "feed";
    private static string RedisKeyFor(long userId) => $"{KeyPrefix}:{userId}";
    
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task PushFeedAsync(long userId, IReadOnlyCollection<FeedPost> feedPosts)
    {
        var redisValues = feedPosts
            .Select(post => new RedisValue(JsonSerializer.Serialize(post)))
            .ToArray();
        var redisKey = RedisKeyFor(userId);

        var tran = _db.CreateTransaction();
        _ = tran.ListLeftPushAsync(redisKey, redisValues);
        _  = tran.ListTrimAsync(redisKey, 0, IFeedService.FeedCapacity - 1);
        await tran.ExecuteAsync();
    }

    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId)
    {
        var cachedFeed = await _db.ListRangeAsync(RedisKeyFor(userId));
        
        return cachedFeed
            .Select(v => JsonSerializer.Deserialize<FeedPost>(v.ToString()))
            .ToArray();
    }
}