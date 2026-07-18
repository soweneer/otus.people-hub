using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IUserRepository
{
    Task<int> GetUserIdAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
