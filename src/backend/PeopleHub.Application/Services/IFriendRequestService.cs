using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFriendRequestService
{
    Task CancelAsync(string email, int otherUserId, CancellationToken cancellationToken = default);

    Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken = default);

    Task SendAsync(string email, int receiverUserId, CancellationToken cancellationToken = default);

    Task<bool> SetFriendAsync(string email, int friendUserId, CancellationToken cancellationToken = default);

    Task ApproveAsync(string email, int requestId, CancellationToken cancellationToken = default);

    Task RejectAsync(string email, int requestId, CancellationToken cancellationToken = default);
}
