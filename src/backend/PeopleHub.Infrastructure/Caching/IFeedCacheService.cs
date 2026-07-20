using PeopleHub.Application.Models;

namespace PeopleHub.Infrastructure.Caching;

public interface IFeedCacheService
{
    Task PushFeedAsync(int userId, IReadOnlyCollection<FeedPost> feedPosts);
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int userId);
}