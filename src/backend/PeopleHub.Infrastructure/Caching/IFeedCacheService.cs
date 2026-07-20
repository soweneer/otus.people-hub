using PeopleHub.Application.Models;

namespace PeopleHub.Infrastructure.Caching;

public interface IFeedCacheService
{
    Task PushFeedAsync(long userId, IReadOnlyCollection<FeedPost> feedPosts);
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId);
    Task AddPostAsync(long userId, FeedPost post);
    Task UpdatePostAsync(long userId, FeedPost post);
    Task RemovePostAsync(long userId, long postId);
}
