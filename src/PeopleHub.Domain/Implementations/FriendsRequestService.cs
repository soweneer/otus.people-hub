using PeopleHub.Domain.Interfaces;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Implementations;

internal sealed class FriendsRequestService(IPersonRepository personRepository, 
    IFriendRequestRepository friendRequestRepository)
    : IFriendRequestService
{
    public async Task DeleteAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken)
    {
        var personId = await personRepository.GetPersonIdAsync(initiatorEmail, cancellationToken);

        await friendRequestRepository.DeleteAsync(personId, receiverPersonId);
    }

    public async Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken)
    {
        var personId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        
        return await friendRequestRepository.GetFriendsAsync(personId);
    }

    public async Task SendAsync(string senderEmail, int receiverPersonId, CancellationToken cancellationToken)
    {
        var senderPersonId = await personRepository.GetPersonIdAsync(senderEmail, cancellationToken);
        
        await friendRequestRepository.SendAsync(senderPersonId, receiverPersonId);
    }
}