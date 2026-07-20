using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Infrastructure.Caching;

namespace PeopleHub.Unit.Tests.Decorators;

public sealed class CachingFeedServiceDecoratorTests
{
    private const string Email = "user@example.com";

    private static readonly TimeSpan ExpectedCacheTtl = TimeSpan.FromMinutes(5);

    private static readonly IReadOnlyCollection<FeedPost> DefaultFeed =
    [
        new(1, "первый пост", 10),
        new(2, "второй пост", 20)
    ];

    private readonly FeedServiceStub _underlyingService = new(DefaultFeed);
    private readonly MemoryDistributedCache _cache = new(Options.Create(new MemoryDistributedCacheOptions()));
    private readonly CachingFeedServiceDecorator _decorator;

    public CachingFeedServiceDecoratorTests()
    {
        _decorator = new CachingFeedServiceDecorator(_underlyingService, _cache);
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_ReturnsFeedFromUnderlyingService()
    {
        var feed = await _decorator.GetFeedAsync(Email);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(Email, _underlyingService.Calls.Single());
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_StoresFeedInCache()
    {
        await _decorator.GetFeedAsync(Email);

        var cachedJson = await _cache.GetStringAsync($"feed:{Email}");

        Assert.NotNull(cachedJson);
        Assert.Equal(DefaultFeed, JsonSerializer.Deserialize<IReadOnlyCollection<FeedPost>>(cachedJson));
    }

    [Fact]
    public async Task GetFeedAsync_SecondCall_DoesNotCallUnderlyingServiceAgain()
    {
        var firstFeed = await _decorator.GetFeedAsync(Email);
        var secondFeed = await _decorator.GetFeedAsync(Email);

        Assert.Single(_underlyingService.Calls);
        Assert.Equal(firstFeed, secondFeed);
    }

    [Fact]
    public async Task GetFeedAsync_CacheHit_ReturnsCachedFeedWithoutCallingUnderlyingService()
    {
        var cachedFeed = new[] { new FeedPost(42, "пост из кэша", 7) };
        await _cache.SetStringAsync($"feed:{Email}", JsonSerializer.Serialize(cachedFeed));

        var feed = await _decorator.GetFeedAsync(Email);

        Assert.Equal(cachedFeed, feed);
        Assert.Empty(_underlyingService.Calls);
    }

    [Fact]
    public async Task GetFeedAsync_DifferentEmails_UseSeparateCacheEntries()
    {
        const string otherEmail = "other@example.com";
        var otherFeed = new[] { new FeedPost(3, "чужой пост", 30) };
        var decorator = new CachingFeedServiceDecorator(
            new FeedServiceStub(email => email == otherEmail ? otherFeed : DefaultFeed),
            _cache);

        var feed = await decorator.GetFeedAsync(Email);
        var feedForOther = await decorator.GetFeedAsync(otherEmail);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(otherFeed, feedForOther);
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_StoresEntryWithTtl()
    {
        var recordingCache = new RecordingCache(_cache);
        var decorator = new CachingFeedServiceDecorator(_underlyingService, recordingCache);

        await decorator.GetFeedAsync(Email);

        Assert.NotNull(recordingCache.LastSetOptions);
        Assert.Equal(ExpectedCacheTtl, recordingCache.LastSetOptions.AbsoluteExpirationRelativeToNow);
    }

    [Fact]
    public async Task GetFeedAsync_AfterTtlExpires_CallsUnderlyingServiceAgain()
    {
        var clock = new TestClock();
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions { Clock = clock }));
        var decorator = new CachingFeedServiceDecorator(_underlyingService, cache);

        await decorator.GetFeedAsync(Email);
        clock.Advance(ExpectedCacheTtl + TimeSpan.FromSeconds(1));
        var feedAfterExpiration = await decorator.GetFeedAsync(Email);

        Assert.Equal(2, _underlyingService.Calls.Count);
        Assert.Equal(DefaultFeed, feedAfterExpiration);
    }

    [Fact]
    public async Task GetFeedAsync_BeforeTtlExpires_StillServesFromCache()
    {
        var clock = new TestClock();
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions { Clock = clock }));
        var decorator = new CachingFeedServiceDecorator(_underlyingService, cache);

        await decorator.GetFeedAsync(Email);
        clock.Advance(ExpectedCacheTtl - TimeSpan.FromSeconds(1));
        await decorator.GetFeedAsync(Email);

        Assert.Single(_underlyingService.Calls);
    }

    [Fact]
    public async Task GetFeedAsync_EmptyFeed_IsCachedToo()
    {
        var underlyingService = new FeedServiceStub([]);
        var decorator = new CachingFeedServiceDecorator(underlyingService, _cache);

        var firstFeed = await decorator.GetFeedAsync(Email);
        var secondFeed = await decorator.GetFeedAsync(Email);

        Assert.Empty(firstFeed);
        Assert.Empty(secondFeed);
        Assert.Single(underlyingService.Calls);
    }

    private sealed class TestClock : Microsoft.Extensions.Internal.ISystemClock
    {
        public DateTimeOffset UtcNow { get; private set; } = DateTimeOffset.UtcNow;

        public void Advance(TimeSpan delta) => UtcNow += delta;
    }

    private sealed class RecordingCache(IDistributedCache inner) : IDistributedCache
    {
        public DistributedCacheEntryOptions LastSetOptions { get; private set; }

        public byte[] Get(string key) => inner.Get(key);

        public Task<byte[]> GetAsync(string key, CancellationToken token = default) => inner.GetAsync(key, token);

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            LastSetOptions = options;
            inner.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            LastSetOptions = options;
            return inner.SetAsync(key, value, options, token);
        }

        public void Refresh(string key) => inner.Refresh(key);

        public Task RefreshAsync(string key, CancellationToken token = default) => inner.RefreshAsync(key, token);

        public void Remove(string key) => inner.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default) => inner.RemoveAsync(key, token);
    }

    private sealed class FeedServiceStub(Func<string, IReadOnlyCollection<FeedPost>> feedByEmail) : IFeedService
    {
        public FeedServiceStub(IReadOnlyCollection<FeedPost> feed) : this(_ => feed)
        {
        }

        public List<string> Calls { get; } = [];

        public Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, CancellationToken cancellationToken = default)
        {
            Calls.Add(email);
            return Task.FromResult(feedByEmail(email));
        }
    }
}
