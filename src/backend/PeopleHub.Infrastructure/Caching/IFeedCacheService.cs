using PeopleHub.Application.Models;

namespace PeopleHub.Infrastructure.Caching;

public interface IFeedCacheService
{
    Task PushFeedAsync(long userId, IReadOnlyCollection<FeedPost> feedPosts);
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId);
}
