using PeopleHub.Application.Models;
using PeopleHub.Application.Services;

namespace PeopleHub.Infrastructure.Caching;

public sealed class CachingFeedServiceDecorator(IFeedService underlyingService, IFeedCacheService cacheService,
    IUserService userService /* TODO КОСТЫЛЬ! убрать user-id в auth claim */)
    : IFeedService
{
    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, CancellationToken cancellationToken = default)
    {
        var userId = await userService.GetUserId(email, cancellationToken);
        var feed = await cacheService.GetFeedAsync(userId);
        if (feed is { Count: > 0 })
            return feed;

        feed = await underlyingService.GetFeedAsync(email, cancellationToken);
        if (feed is { Count: > 0 })
        {
            await cacheService.PushFeedAsync(userId, feed);
        }

        return feed;
    }
}
