using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PersonService(IPersonRepository personRepository) : IPersonService
{
    public async Task<IReadOnlyCollection<PersonInfo>> GetFriendsAsync(string email, CancellationToken cancellationToken = default)
    {
        var persons = await personRepository.GetFriendsAsync(email, cancellationToken);
        return persons.Select(ToDto).ToArray();
    }

    public async Task<PersonInfo?> GetByIdAsync(string email, int? targetPersonId, CancellationToken cancellationToken = default)
    {
        var curPersonId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        var id = targetPersonId ?? curPersonId;
        var viewerId = targetPersonId.HasValue ? (int?)curPersonId : null;
        var person = await personRepository.GetByIdAsync(id, viewerId, cancellationToken);
        return person is null ? null : ToDto(person);
    }

    public async Task<PersonInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var currentUserPersonId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        var updatedPerson = await personRepository.UpdateAsync(currentUserPersonId, personalInfo, cancellationToken);
        return ToDto(updatedPerson);
    }

    private static PersonInfo ToDto(Person p) => new(p.Id, p.Name, p.Surname, p.Age, p.City, p.Gender, p.Bio, p.Status);
}