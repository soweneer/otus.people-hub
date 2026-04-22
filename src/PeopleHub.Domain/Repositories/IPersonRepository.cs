using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IPersonRepository
{
    Task<int> GetPersonIdAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PersonInfo>> GetAllAsync(string currentUserEmail, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PersonInfo>> SearchAsync(string currentUserEmail, string surname, string name, CancellationToken cancellationToken);
    Task<Friend> GetByIdAsync(int personId, int viewerPersonId, CancellationToken cancellationToken);
    Task<PersonalInfo> GetAsync(int personId, CancellationToken cancellationToken);
    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken);
    Task UpdateAsync(int personId, PersonalInfo personalInfo, CancellationToken cancellationToken);
}
