using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

internal sealed class FriendsRequestService(IPersonRepository personRepository, 
    IFriendRepository friendRepository)
    : IFriendRequestService
{
    public async Task DeleteAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken)
    {
        var personId = await personRepository.GetPersonIdAsync(initiatorEmail, cancellationToken);

        await friendRepository.DeleteAsync(personId, receiverPersonId);
    }

    public async Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken)
    {
        var personId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        
        return await friendRepository.GetFriendsAsync(personId);
    }

    public async Task SendAsync(string senderEmail, int receiverPersonId, CancellationToken cancellationToken)
    {
        var senderPersonId = await personRepository.GetPersonIdAsync(senderEmail, cancellationToken);
        
        await friendRepository.SendAsync(senderPersonId, receiverPersonId);
    }
}