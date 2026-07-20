using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
