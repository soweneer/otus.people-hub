using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task<FriendRequest> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<FriendRequest> FindBetweenAsync(int userId, int otherUserId, CancellationToken cancellationToken = default);

    Task AddAsync(FriendRequest request, CancellationToken cancellationToken = default);

    Task SaveStatusAsync(FriendRequest request, CancellationToken cancellationToken = default);

    Task DeleteBetweenAsync(int userId, int otherUserId, CancellationToken cancellationToken = default);
}
