using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IFeedRepository
{
    Task<IReadOnlyCollection<Post>> GetFriendsFeedAsync(long userId, int offset, int limit);
}