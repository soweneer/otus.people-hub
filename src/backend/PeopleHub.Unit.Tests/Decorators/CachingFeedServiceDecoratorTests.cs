using Microsoft.Extensions.Options;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Infrastructure;
using PeopleHub.Infrastructure.Caching;

namespace PeopleHub.Unit.Tests.Decorators;

public sealed class CachingFeedServiceDecoratorTests
{
    private const long UserId = 42;

    private static readonly IReadOnlyCollection<FeedPost> DefaultFeed =
    [
        new(1, "первый пост", 10),
        new(2, "второй пост", 20)
    ];

    private readonly FeedServiceStub _underlyingService = new(DefaultFeed);
    private readonly FeedCacheServiceStub _cacheService = new();
    private readonly CachingFeedServiceDecorator _decorator;

    public CachingFeedServiceDecoratorTests()
    {
        _decorator = new CachingFeedServiceDecorator(_underlyingService, _cacheService, new FeatureFlagsStub(true));
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_ReturnsFeedFromUnderlyingService()
    {
        var feed = await _decorator.GetFeedAsync(UserId);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(UserId, _underlyingService.Calls.Single());
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_PushesFeedToCache()
    {
        await _decorator.GetFeedAsync(UserId);

        var (userId, feed) = Assert.Single(_cacheService.Pushes);
        Assert.Equal(UserId, userId);
        Assert.Equal(DefaultFeed, feed);
    }

    [Fact]
    public async Task GetFeedAsync_SecondCall_DoesNotCallUnderlyingServiceAgain()
    {
        var firstFeed = await _decorator.GetFeedAsync(UserId);
        var secondFeed = await _decorator.GetFeedAsync(UserId);

        Assert.Single(_underlyingService.Calls);
        Assert.Equal(firstFeed, secondFeed);
    }

    [Fact]
    public async Task GetFeedAsync_CacheHit_ReturnsCachedFeedWithoutCallingUnderlyingService()
    {
        var cachedFeed = new[] { new FeedPost(42, "пост из кэша", 7) };
        await _cacheService.PushFeedAsync(UserId, cachedFeed);

        var feed = await _decorator.GetFeedAsync(UserId);

        Assert.Equal(cachedFeed, feed);
        Assert.Empty(_underlyingService.Calls);
    }

    [Fact]
    public async Task GetFeedAsync_EmptyFeedInCache_FallsBackToUnderlyingService()
    {
        await _cacheService.PushFeedAsync(UserId, []);

        var feed = await _decorator.GetFeedAsync(UserId);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(UserId, _underlyingService.Calls.Single());
    }

    [Fact]
    public async Task GetFeedAsync_EmptyFeed_IsNotCached()
    {
        var underlyingService = new FeedServiceStub([]);
        var decorator = new CachingFeedServiceDecorator(underlyingService, _cacheService, new FeatureFlagsStub(true));

        var firstFeed = await decorator.GetFeedAsync(UserId);
        var secondFeed = await decorator.GetFeedAsync(UserId);

        Assert.Empty(firstFeed);
        Assert.Empty(secondFeed);
        Assert.Empty(_cacheService.Pushes);
        Assert.Equal(2, underlyingService.Calls.Count);
    }

    [Fact]
    public async Task GetFeedAsync_CacheDisabledByFeatureFlag_BypassesCache()
    {
        var decorator = new CachingFeedServiceDecorator(_underlyingService, _cacheService, new FeatureFlagsStub(false));
        await _cacheService.PushFeedAsync(UserId, [new FeedPost(99, "устаревший пост", 9)]);
        _cacheService.Pushes.Clear();

        var feed = await decorator.GetFeedAsync(UserId);
        await decorator.GetFeedAsync(UserId);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(2, _underlyingService.Calls.Count);
        Assert.Empty(_cacheService.Pushes);
    }

    [Fact]
    public async Task GetFeedAsync_DifferentUsers_UseSeparateCacheEntries()
    {
        const long otherUserId = 43;
        var otherFeed = new[] { new FeedPost(3, "чужой пост", 30) };
        var decorator = new CachingFeedServiceDecorator(
            new FeedServiceStub(userId => userId == otherUserId ? otherFeed : DefaultFeed),
            _cacheService,
            new FeatureFlagsStub(true));

        var feed = await decorator.GetFeedAsync(UserId);
        var feedForOther = await decorator.GetFeedAsync(otherUserId);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(otherFeed, feedForOther);
        Assert.Equal(DefaultFeed, await _cacheService.GetFeedAsync(UserId));
        Assert.Equal(otherFeed, await _cacheService.GetFeedAsync(otherUserId));
    }

    private sealed class FeatureFlagsStub(bool useCacheForFeed) : IOptionsMonitor<FeatureFlagsOptions>
    {
        public FeatureFlagsOptions CurrentValue { get; } = new() { UseCacheForFeed = useCacheForFeed };

        public FeatureFlagsOptions Get(string name) => CurrentValue;

        public IDisposable OnChange(Action<FeatureFlagsOptions, string> listener) => null;
    }

    private sealed class FeedCacheServiceStub : IFeedCacheService
    {
        private readonly Dictionary<long, IReadOnlyCollection<FeedPost>> _store = [];

        public List<(long UserId, IReadOnlyCollection<FeedPost> Feed)> Pushes { get; } = [];

        public Task PushFeedAsync(long userId, IReadOnlyCollection<FeedPost> feedPosts)
        {
            Pushes.Add((userId, feedPosts));
            _store[userId] = feedPosts;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId) =>
            Task.FromResult(_store.GetValueOrDefault(userId, []));

        public Task AddPostAsync(long userId, FeedPost post)
        {
            if (_store.TryGetValue(userId, out var feed))
            {
                _store[userId] = [post, .. feed];
            }

            return Task.CompletedTask;
        }

        public Task UpdatePostAsync(long userId, FeedPost post)
        {
            if (_store.TryGetValue(userId, out var feed))
            {
                _store[userId] = [.. feed.Select(p => p.Id == post.Id ? post : p)];
            }

            return Task.CompletedTask;
        }

        public Task RemovePostAsync(long userId, long postId)
        {
            if (_store.TryGetValue(userId, out var feed))
            {
                _store[userId] = [.. feed.Where(p => p.Id != postId)];
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FeedServiceStub(Func<long, IReadOnlyCollection<FeedPost>> feedByUserId) : IFeedService
    {
        public FeedServiceStub(IReadOnlyCollection<FeedPost> feed) : this(_ => feed)
        {
        }

        public List<long> Calls { get; } = [];

        public Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId, CancellationToken cancellationToken = default)
        {
            Calls.Add(userId);
            return Task.FromResult(feedByUserId(userId));
        }
    }
}
