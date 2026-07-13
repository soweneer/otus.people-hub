using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task ApproveAsync(int id, int receiverPersonId);
    
    Task DeleteAsync(int id, int receiverPersonId);
    
    Task<FriendsInfo> GetFriendsAsync(int userId);
    
    Task<FriendRequest> GetAsync(int id);
    
    Task RejectAsync(int id, int receiverPersonId);
    
    Task SendAsync(int senderPersonId, int receiverPersonId);

    Task SetFriendAsync(int userId, int friendUserId);
}