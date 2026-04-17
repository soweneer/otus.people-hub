using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IPersonRepository
{
    Task<int> GetPersonIdAsync(string email, CancellationToken cancellationToken);

    Task<int?> CreateAsync(Person person, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Person>> GetFriendsAsync(string currentUserEmail, CancellationToken cancellationToken);

    Task<Person> GetByIdAsync(int personId, int? currentPersonId, CancellationToken cancellationToken);

    Task<Person> UpdateAsync(int personId, UpdatePersonData personData, CancellationToken cancellationToken);
}
