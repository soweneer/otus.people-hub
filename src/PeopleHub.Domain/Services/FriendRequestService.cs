using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

internal sealed class FriendRequestService(IPersonRepository personRepository, 
    IFriendRequestRepository friendRequestRepository)
    : IFriendRequestService
{
    public async Task CancelAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken)
    {
        var senderPersonId = await personRepository.GetPersonIdAsync(initiatorEmail, cancellationToken);

        await friendRequestRepository.DeleteAsync(senderPersonId, receiverPersonId);
    }

    public async Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken)
    {
        var personId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        
        return await friendRequestRepository.GetFriendsAsync(personId);
    }

    public async Task SendAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken)
    {
        var senderPersonId = await personRepository.GetPersonIdAsync(initiatorEmail, cancellationToken);
        
        await friendRequestRepository.SendAsync(senderPersonId, receiverPersonId);
    }

    public async Task ApproveAsync(string receiverEmail, int id, CancellationToken cancellationToken)
    {
        var receiverPersonId = await personRepository.GetPersonIdAsync(receiverEmail, cancellationToken);
        
        await friendRequestRepository.ApproveAsync(id, receiverPersonId);
    }

    public async Task RejectAsync(string receiverEmail, int id, CancellationToken cancellationToken)
    {
        var receiverPersonId = await personRepository.GetPersonIdAsync(receiverEmail, cancellationToken);
        
        await friendRequestRepository.RejectAsync(id, receiverPersonId);
    }
}