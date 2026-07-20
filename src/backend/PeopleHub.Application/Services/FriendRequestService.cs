using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

internal sealed class FriendRequestService(IUserRepository userRepository,
    IFriendRequestRepository friendRequestRepository,
    IFriendQueries friendQueries) : IFriendRequestService
{
    public async Task CancelAsync(long userId, long otherUserId, CancellationToken cancellationToken = default) => 
        await friendRequestRepository.DeleteBetweenAsync(userId, otherUserId, cancellationToken);

    public async Task<FriendsInfo> GetFriendsAsync(long userId, CancellationToken cancellationToken = default) => 
        await friendQueries.GetFriendsAsync(userId, cancellationToken);

    public async Task SendAsync(long userId, long receiverUserId, CancellationToken cancellationToken = default) => 
        await friendRequestRepository.AddAsync(FriendRequest.Send(userId, receiverUserId), cancellationToken);

    public async Task<bool> SetFriendAsync(long userId, long friendUserId, CancellationToken cancellationToken = default)
    {
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

    public async Task ApproveAsync(long userId, long requestId, CancellationToken cancellationToken = default)
    {
        var request = await friendRequestRepository.GetAsync(requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Approve(userId);
        await friendRequestRepository.SaveStatusAsync(request, cancellationToken);
    }

    public async Task RejectAsync(long userId, long requestId, CancellationToken cancellationToken = default)
    {
        var request = await friendRequestRepository.GetAsync(requestId, cancellationToken);
        if (request is null)
        {
            return;
        }

        request.Reject(userId);
        await friendRequestRepository.SaveStatusAsync(request, cancellationToken);
    }
}
