using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IPersonRepository
{
    Task<int> GetPersonIdAsync(string email, CancellationToken cancellationToken);
}