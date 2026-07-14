using PeopleHub.Application.Models;

namespace PeopleHub.Application.Abstractions;

/// <summary>
/// Read-сторона: поисковые запросы по пользователям, минуя доменные агрегаты.
/// </summary>
public interface IUserQueries
{
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(int viewerUserId, SearchFilter filter,
        CancellationToken cancellationToken = default);

    Task<FriendInfo> GetWithFriendStatusAsync(int userId, int viewerUserId, CancellationToken cancellationToken = default);
}
