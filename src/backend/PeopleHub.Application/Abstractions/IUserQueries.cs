using PeopleHub.Application.Models;

namespace PeopleHub.Application.Abstractions;

public interface IUserQueries
{
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(long viewerUserId, SearchFilter filter,
        CancellationToken cancellationToken = default);

    Task<FriendInfo> GetWithFriendStatusAsync(long userId, long viewerUserId, CancellationToken cancellationToken = default);
}
