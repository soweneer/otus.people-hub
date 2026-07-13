using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

internal sealed class FriendRequestService(IUserRepository userRepository,
    IFriendRequestRepository friendRequestRepository)
    : IFriendRequestService
{
    public async Task CancelAsync(string initiatorEmail, int receiverUserId, CancellationToken cancellationToken)
    {
        var senderUserId = await userRepository.GetUserIdAsync(initiatorEmail, cancellationToken);

        await friendRequestRepository.DeleteAsync(senderUserId, receiverUserId);
    }

    public async Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await friendRequestRepository.GetFriendsAsync(userId);
    }

    public async Task SendAsync(string initiatorEmail, int receiverUserId, CancellationToken cancellationToken)
    {
        var senderUserId = await userRepository.GetUserIdAsync(initiatorEmail, cancellationToken);

        await friendRequestRepository.SendAsync(senderUserId, receiverUserId);
    }

    public async Task<bool> SetFriendAsync(string initiatorEmail, int friendUserId, CancellationToken cancellationToken)
    {
        var userId = await userRepository.GetUserIdAsync(initiatorEmail, cancellationToken);
        if (userId == friendUserId || await userRepository.GetAsync(friendUserId, cancellationToken) is null)
        {
            return false;
        }

        await friendRequestRepository.SetFriendAsync(userId, friendUserId);
        return true;
    }

    public async Task ApproveAsync(string receiverEmail, int id, CancellationToken cancellationToken)
    {
        var receiverUserId = await userRepository.GetUserIdAsync(receiverEmail, cancellationToken);

        await friendRequestRepository.ApproveAsync(id, receiverUserId);
    }

    public async Task RejectAsync(string receiverEmail, int id, CancellationToken cancellationToken)
    {
        var receiverUserId = await userRepository.GetUserIdAsync(receiverEmail, cancellationToken);

        await friendRequestRepository.RejectAsync(id, receiverUserId);
    }
}
