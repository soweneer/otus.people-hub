using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IPersonService
{
    Task<Person> UpdateAsync(string email, UpdatePersonData personData, CancellationToken cancellationToken = default);
}