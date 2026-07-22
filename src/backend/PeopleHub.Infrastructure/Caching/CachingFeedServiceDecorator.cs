using Microsoft.Extensions.Options;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;

namespace PeopleHub.Infrastructure.Caching;

public sealed class CachingFeedServiceDecorator(IFeedService underlyingService, IFeedCacheService cacheService,
    IOptionsMonitor<FeatureFlagsOptions> featureFlags) : IFeedService
{
    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId, CancellationToken cancellationToken = default)
    {
        if (!featureFlags.CurrentValue.UseCacheForFeed)
        {
            return await underlyingService.GetFeedAsync(userId, cancellationToken);
        }

        var feed = await cacheService.GetFeedAsync(userId);
        if (feed is { Count: > 0 })
        {
            FeedCacheMetrics.HitsCounter.Inc();
            return feed;
        }

        FeedCacheMetrics.MissCounter.Inc();
        feed = await underlyingService.GetFeedAsync(userId, cancellationToken);
        if (feed is { Count: > 0 })
        {
            await cacheService.PushFeedAsync(userId, feed);
        }

        return feed;
    }
}
