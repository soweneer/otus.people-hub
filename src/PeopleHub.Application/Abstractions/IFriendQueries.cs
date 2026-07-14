using PeopleHub.Application.Models;

namespace PeopleHub.Application.Abstractions;

public interface IFriendQueries
{
    Task<FriendsInfo> GetFriendsAsync(int userId, CancellationToken cancellationToken = default);
}
