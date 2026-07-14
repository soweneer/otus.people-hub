using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IAccountRepository
{
    Task<int?> CreateAsync(string email, string password, int userId);

    Task<bool> ExistsAsync(string email);

    Task<Account> FindByEmailAsync(string email);

    Task<Account> FindByUserIdAsync(int userId);
}