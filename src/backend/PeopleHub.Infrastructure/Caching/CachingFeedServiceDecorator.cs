using PeopleHub.Application.Models;
using PeopleHub.Application.Services;

namespace PeopleHub.Infrastructure.Caching;

public sealed class CachingFeedServiceDecorator(IFeedService underlyingService, IFeedCacheService cacheService) : IFeedService
{
    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId, CancellationToken cancellationToken = default)
    {
        var feed = await cacheService.GetFeedAsync(userId);
        if (feed is { Count: > 0 })
            return feed;

        feed = await underlyingService.GetFeedAsync(userId, cancellationToken);
        if (feed is { Count: > 0 })
        {
            await cacheService.PushFeedAsync(userId, feed);
        }

        return feed;
    }
}
