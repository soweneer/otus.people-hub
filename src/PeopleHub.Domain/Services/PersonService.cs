using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PersonService(IPersonRepository personRepository): IPersonService
{
    public async Task<Person> UpdateAsync(string email, UpdatePersonData personData, CancellationToken cancellationToken = default)
    {
        var currentUserPersonId = await personRepository.GetPersonIdAsync(email, cancellationToken);

        var updatedPerson = await personRepository.UpdateAsync(currentUserPersonId, personData,
            cancellationToken);
        
        return updatedPerson;
    }
}