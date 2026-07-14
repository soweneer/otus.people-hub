using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

internal sealed class FriendRequestService(IUserRepository userRepository,
    IFriendRequestRepository friendRequestRepository,
    IFriendQueries friendQueries,
    ICurrentUser currentUser) : IFriendRequestService
{
    public async Task CancelAsync(int otherUserId, CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        await friendRequestRepository.DeleteBetweenAsync(userId, otherUserId, cancellationToken);
    }

    public async Task<FriendsInfo> GetFriendsAsync(CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        return await friendQueries.GetFriendsAsync(userId, cancellationToken);
    }

    public async Task SendAsync(int receiverUserId, CancellationToken cancellationToken = default)
    {
        var senderUserId = await currentUser.GetUserIdAsync(cancellationToken);

        await friendRequestRepository.AddAsync(FriendRequest.Send(senderUserId, receiverUserId), cancellationToken);
    }

    public async Task<bool> SetFriendAsync(int friendUserId, CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);
        if (userId == friendUserId || await userRepository.GetAsync(friendUserId, cancellationToken) is null)
        {
            return false;
        }

        var existingRequest = await friendRequestRepository.FindBetweenAsync(userId, friendUserId, cancellationToken);
        if (existingRequest is null)
        {
            await friendRequestRepository.AddAsync(FriendRequest.EstablishFriendship(userId, friendUserId), cancellationToken);
        }
        else
        {
            existingRequest.MarkApproved();
            await friendRequestRepository.SaveStatusAsync(existingRequest, cancellationToken);
        }

        return true;
    }

    public async Task ApproveAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var receiverUserId = await currentUser.GetUserIdAsync(cancellationToken);

        var request = await friendRequestRepository.GetAsync(requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Approve(receiverUserId);
        await friendRequestRepository.SaveStatusAsync(request, cancellationToken);
    }

    public async Task RejectAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var receiverUserId = await currentUser.GetUserIdAsync(cancellationToken);

        var request = await friendRequestRepository.GetAsync(requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Reject(receiverUserId);
        await friendRequestRepository.SaveStatusAsync(request, cancellationToken);
    }
}
