using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Interfaces;

public interface IFriendRequestService
{
    Task DeleteAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken);
    
    Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken);
}