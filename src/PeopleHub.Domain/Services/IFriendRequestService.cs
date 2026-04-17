using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IFriendRequestService
{
    Task CancelAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken);
    
    Task<FriendsInfo> GetFriendsAsync(string email, CancellationToken cancellationToken);

    Task SendAsync(string initiatorEmail, int receiverPersonId, CancellationToken cancellationToken);
    
    Task ApproveAsync(string receiverEmail, int id, CancellationToken cancellationToken);
    
    Task RejectAsync(string receiverEmail, int id, CancellationToken httpContextRequestAborted);
}