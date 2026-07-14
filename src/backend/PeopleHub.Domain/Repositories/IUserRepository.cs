using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IUserRepository
{
    Task<int> GetUserIdAsync(string email, CancellationToken cancellationToken = default);

    Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
