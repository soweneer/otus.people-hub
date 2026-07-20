using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IFeedRepository
{
    Task<IReadOnlyCollection<Post>> GetFriendsFeedAsync(int userId, int offset, int limit);
}