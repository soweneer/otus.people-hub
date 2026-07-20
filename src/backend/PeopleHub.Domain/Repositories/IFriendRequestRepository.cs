using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IFriendRequestRepository
{
    Task<FriendRequest> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<FriendRequest> FindBetweenAsync(long userId, long otherUserId, CancellationToken cancellationToken = default);

    Task AddAsync(FriendRequest request, CancellationToken cancellationToken = default);

    Task SaveStatusAsync(FriendRequest request, CancellationToken cancellationToken = default);

    Task DeleteBetweenAsync(long userId, long otherUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<long>> GetFriendIdsAsync(long userId, CancellationToken cancellationToken = default);
}
