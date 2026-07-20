using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFriendRequestService
{
    Task CancelAsync(long userId, long otherUserId, CancellationToken cancellationToken = default);

    Task<FriendsInfo> GetFriendsAsync(long userId, CancellationToken cancellationToken = default);

    Task SendAsync(long userId, long receiverUserId, CancellationToken cancellationToken = default);

    Task<bool> SetFriendAsync(long userId, long friendUserId, CancellationToken cancellationToken = default);

    Task ApproveAsync(long userId, long requestId, CancellationToken cancellationToken = default);

    Task RejectAsync(long userId, long requestId, CancellationToken cancellationToken = default);
}
