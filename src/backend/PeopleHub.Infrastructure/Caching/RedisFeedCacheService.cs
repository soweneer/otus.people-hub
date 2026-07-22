using System.Text.Json;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using StackExchange.Redis;

namespace PeopleHub.Infrastructure.Caching;

public class RedisFeedCacheService(IConnectionMultiplexer redis) : IFeedCacheService
{
    private const string KeyPrefix = "feed";
    private static string FeedKeyFor(long userId) => $"{KeyPrefix}:{userId}";
    private static RedisValue Serialize(FeedPost post) => new(JsonSerializer.Serialize(post));
    private static FeedPost Deserialize(RedisValue value) => JsonSerializer.Deserialize<FeedPost>(value.ToString());

    private readonly IDatabase _db = redis.GetDatabase();

    public async Task PushFeedAsync(long userId, IReadOnlyCollection<FeedPost> feedPosts)
    {
        var redisValues = feedPosts
            .Select(Serialize)
            .ToArray();
        var redisKey = FeedKeyFor(userId);

        var tran = _db.CreateTransaction();
        _ = tran.KeyDeleteAsync(redisKey);
        _ = tran.ListRightPushAsync(redisKey, redisValues);
        _ = tran.ListTrimAsync(redisKey, 0, IFeedService.FeedCapacity - 1);
        await tran.ExecuteAsync();
    }

    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId)
    {
        var cachedFeed = await _db.ListRangeAsync(FeedKeyFor(userId));

        return cachedFeed
            .Select(Deserialize)
            .ToArray();
    }

    public async Task AddPostAsync(long userId, FeedPost post)
    {
        var redisKey = FeedKeyFor(userId);
        var length = await _db.ListLeftPushAsync(redisKey, Serialize(post), When.Exists);
        if (length > IFeedService.FeedCapacity)
        {
            await _db.ListTrimAsync(redisKey, 0, IFeedService.FeedCapacity - 1);
        }
    }

    public async Task UpdatePostAsync(long userId, FeedPost post)
    {
        var redisKey = FeedKeyFor(userId);
        var values = await _db.ListRangeAsync(redisKey);
        for (var i = 0; i < values.Length; i++)
        {
            if (Deserialize(values[i]).Id == post.Id)
            {
                await _db.ListSetByIndexAsync(redisKey, i, Serialize(post));
                return;
            }
        }
    }

    public async Task RemovePostAsync(long userId, long postId)
    {
        var redisKey = FeedKeyFor(userId);
        var values = await _db.ListRangeAsync(redisKey);
        foreach (var value in values)
        {
            if (Deserialize(value).Id == postId)
            {
                await _db.ListRemoveAsync(redisKey, value);
                return;
            }
        }
    }
}
