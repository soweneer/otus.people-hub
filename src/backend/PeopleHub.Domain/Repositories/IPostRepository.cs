using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IPostRepository
{
    Task<Post> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<long?> AddAsync(Post post, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Post post, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long id, long authorUserId, CancellationToken cancellationToken = default);
}
