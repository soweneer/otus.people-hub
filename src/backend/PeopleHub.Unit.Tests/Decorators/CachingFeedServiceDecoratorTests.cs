using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Model;
using PeopleHub.Infrastructure.Caching;

namespace PeopleHub.Unit.Tests.Decorators;

public sealed class CachingFeedServiceDecoratorTests
{
    private const string Email = "user@example.com";
    private const int UserId = 42;

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
        _decorator = new CachingFeedServiceDecorator(_underlyingService, _cacheService,
            new UserServiceStub(new Dictionary<string, int> { [Email] = UserId }));
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_ReturnsFeedFromUnderlyingService()
    {
        var feed = await _decorator.GetFeedAsync(Email);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(Email, _underlyingService.Calls.Single());
    }

    [Fact]
    public async Task GetFeedAsync_CacheMiss_PushesFeedToCache()
    {
        await _decorator.GetFeedAsync(Email);

        var (userId, feed) = Assert.Single(_cacheService.Pushes);
        Assert.Equal(UserId, userId);
        Assert.Equal(DefaultFeed, feed);
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
        await _cacheService.PushFeedAsync(UserId, cachedFeed);

        var feed = await _decorator.GetFeedAsync(Email);

        Assert.Equal(cachedFeed, feed);
        Assert.Empty(_underlyingService.Calls);
    }

    [Fact]
    public async Task GetFeedAsync_EmptyFeedInCache_FallsBackToUnderlyingService()
    {
        await _cacheService.PushFeedAsync(UserId, []);

        var feed = await _decorator.GetFeedAsync(Email);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(Email, _underlyingService.Calls.Single());
    }

    [Fact]
    public async Task GetFeedAsync_EmptyFeed_IsNotCached()
    {
        var underlyingService = new FeedServiceStub([]);
        var decorator = new CachingFeedServiceDecorator(underlyingService, _cacheService,
            new UserServiceStub(new Dictionary<string, int> { [Email] = UserId }));

        var firstFeed = await decorator.GetFeedAsync(Email);
        var secondFeed = await decorator.GetFeedAsync(Email);

        Assert.Empty(firstFeed);
        Assert.Empty(secondFeed);
        Assert.Empty(_cacheService.Pushes);
        Assert.Equal(2, underlyingService.Calls.Count);
    }

    [Fact]
    public async Task GetFeedAsync_DifferentUsers_UseSeparateCacheEntries()
    {
        const string otherEmail = "other@example.com";
        const int otherUserId = 43;
        var otherFeed = new[] { new FeedPost(3, "чужой пост", 30) };
        var decorator = new CachingFeedServiceDecorator(
            new FeedServiceStub(email => email == otherEmail ? otherFeed : DefaultFeed),
            _cacheService,
            new UserServiceStub(new Dictionary<string, int> { [Email] = UserId, [otherEmail] = otherUserId }));

        var feed = await decorator.GetFeedAsync(Email);
        var feedForOther = await decorator.GetFeedAsync(otherEmail);

        Assert.Equal(DefaultFeed, feed);
        Assert.Equal(otherFeed, feedForOther);
        Assert.Equal(DefaultFeed, await _cacheService.GetFeedAsync(UserId));
        Assert.Equal(otherFeed, await _cacheService.GetFeedAsync(otherUserId));
    }

    private sealed class FeedCacheServiceStub : IFeedCacheService
    {
        private readonly Dictionary<int, IReadOnlyCollection<FeedPost>> _store = [];

        public List<(int UserId, IReadOnlyCollection<FeedPost> Feed)> Pushes { get; } = [];

        public Task PushFeedAsync(int userId, IReadOnlyCollection<FeedPost> feedPosts)
        {
            Pushes.Add((userId, feedPosts));
            _store[userId] = feedPosts;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int userId) =>
            Task.FromResult(_store.GetValueOrDefault(userId, []));
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

    private sealed class UserServiceStub(IReadOnlyDictionary<string, int> userIdByEmail) : IUserService
    {
        public Task<int> GetUserId(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(userIdByEmail[email]);

        public Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(string email, SearchFilter filter, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<FriendInfo> GetWithFriendStatusAsync(string email, int targetUserId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<PersonalInfo> UpdateProfileAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
