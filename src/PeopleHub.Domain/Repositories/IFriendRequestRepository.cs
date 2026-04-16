using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task ApproveAsync(int id);
    
    Task DeleteAsync(int senderPersonId, int receiverPersonId);
    
    Task<FriendsInfo> GetFriendsAsync(int personId);
    
    Task<FriendRequest> GetAsync(int id);
    
    Task RejectAsync(int id);
    
    Task SendAsync(int senderPersonId, int receiverPersonId);
}