using PeopleHub.Domain.Entities;
using PeopleHub.Domain.ValueObjects;

namespace PeopleHub.Domain.Repositories;

public interface IAccountRepository
{
    Task<int> CreateAsync(Account account, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);

    Task<Account> FindByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<Account> FindByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
