using PeopleHub.Application.Models;

namespace PeopleHub.Application.Abstractions;

public interface IFriendQueries
{
    Task<FriendsInfo> GetFriendsAsync(long userId, CancellationToken cancellationToken = default);
}
