using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFriendRequestService
{
    Task CancelAsync(int otherUserId, CancellationToken cancellationToken = default);

    Task<FriendsInfo> GetFriendsAsync(CancellationToken cancellationToken = default);

    Task SendAsync(int receiverUserId, CancellationToken cancellationToken = default);

    Task<bool> SetFriendAsync(int friendUserId, CancellationToken cancellationToken = default);

    Task ApproveAsync(int requestId, CancellationToken cancellationToken = default);

    Task RejectAsync(int requestId, CancellationToken cancellationToken = default);
}
