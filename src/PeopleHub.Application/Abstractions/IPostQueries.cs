using PeopleHub.Application.Models;

namespace PeopleHub.Application.Abstractions;

public interface IPostQueries
{
    Task<IReadOnlyCollection<FeedPost>> GetFriendsFeedAsync(int userId, int offset, int limit,
        CancellationToken cancellationToken = default);
}
