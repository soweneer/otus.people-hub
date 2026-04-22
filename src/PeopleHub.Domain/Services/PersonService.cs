using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PersonService(IPersonRepository personRepository) : IPersonService
{
    public Task<IReadOnlyCollection<PersonInfo>> GetAllAsync(string email, CancellationToken cancellationToken = default) =>
        personRepository.GetAllAsync(email, cancellationToken);

    public Task<IReadOnlyCollection<PersonInfo>> SearchAsync(string email, string firstName, string lastName,
        CancellationToken cancellationToken = default) =>
        personRepository.SearchAsync(email, lastName, firstName, cancellationToken);

    public async Task<FriendInfo?> GetByEmailAsync(string email, int targetPersonId, CancellationToken cancellationToken = default)
    {
        var viewerId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        var friend = await personRepository.GetByIdAsync(targetPersonId, viewerId, cancellationToken);
        return friend is null 
            ? null
            : ToFriendInfo(friend);
    }

    public async Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default)
    {
        var personId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        
        return await personRepository.GetAsync(personId, cancellationToken);
    }

    public async Task<PersonalInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var currentUserPersonId = await personRepository.GetPersonIdAsync(email, cancellationToken);

        await personRepository.UpdateAsync(currentUserPersonId, personalInfo, cancellationToken);

        return personalInfo;
    }

    private static FriendInfo ToFriendInfo(Friend p) => new(p.Id, p.Name, p.Surname, p.Age, p.City, p.Gender, p.Bio, p.Status);
}