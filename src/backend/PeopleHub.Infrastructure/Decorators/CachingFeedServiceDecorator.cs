using Microsoft.Extensions.Caching.Distributed;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using System.Text.Json;

namespace PeopleHub.Infrastructure.Decorators;

public sealed class CachingFeedServiceDecorator(IFeedService underlyingService, IDistributedCache cache) : IFeedService
{
    internal static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, CancellationToken cancellationToken = default)
    {
        var feedKey = $"feed:{email}";
        var feedJson = await cache.GetStringAsync(feedKey, token: cancellationToken);
        if (feedJson != null)
            return JsonSerializer.Deserialize<IReadOnlyCollection<FeedPost>>(feedJson);

        var feed = await underlyingService.GetFeedAsync(email, cancellationToken);
        feedJson = JsonSerializer.Serialize(feed);
        await cache.SetStringAsync(feedKey, feedJson,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl },
            cancellationToken);
        return feed;
    }
}
