using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

internal sealed class FriendRequestService(IUserRepository userRepository,
    IFriendRequestRepository friendRequestRepository,
    IFriendQueries friendQueries) : IFriendRequestService
{
    public async Task CancelAsync(string email, int otherUserId, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        await friendRequestRepository.DeleteBetweenAsync(userId, otherUserId, cancellationToken);
    }

    public async Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await friendQueries.GetFriendsAsync(userId, cancellationToken);
    }

    public async Task SendAsync(string email, int receiverUserId, CancellationToken cancellationToken = default)
    {
        var senderUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        await friendRequestRepository.AddAsync(FriendRequest.Send(senderUserId, receiverUserId), cancellationToken);
    }

    public async Task<bool> SetFriendAsync(string email, int friendUserId, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);
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

    public async Task ApproveAsync(string email, int requestId, CancellationToken cancellationToken = default)
    {
        var receiverUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var request = await friendRequestRepository.GetAsync(requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Approve(receiverUserId);
        await friendRequestRepository.SaveStatusAsync(request, cancellationToken);
    }

    public async Task RejectAsync(string email, int requestId, CancellationToken cancellationToken = default)
    {
        var receiverUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var request = await friendRequestRepository.GetAsync(requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Reject(receiverUserId);
        await friendRequestRepository.SaveStatusAsync(request, cancellationToken);
    }
}
