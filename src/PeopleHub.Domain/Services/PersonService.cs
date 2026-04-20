using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PersonService(IPersonRepository personRepository) : IPersonService
{
    public async Task<IReadOnlyCollection<FriendInfo>> GetFriendsAsync(string email, CancellationToken cancellationToken = default)
    {
        var persons = await personRepository.GetFriendsAsync(email, cancellationToken);
        return persons.Select(ToFriendInfo).ToArray();
    }

    public async Task<FriendInfo?> GetByIdAsync(string email, int? targetPersonId, CancellationToken cancellationToken = default)
    {
        var curPersonId = await personRepository.GetPersonIdAsync(email, cancellationToken);
        var id = targetPersonId ?? curPersonId;
        var viewerId = targetPersonId.HasValue ? (int?)curPersonId : null;
        var friend = await personRepository.GetByIdAsync(id, viewerId, cancellationToken);
        return friend is null ? null : ToFriendInfo(friend);
    }

    public async Task<PersonalInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var currentUserPersonId = await personRepository.GetPersonIdAsync(email, cancellationToken);

        await personRepository.UpdateAsync(currentUserPersonId, personalInfo, cancellationToken);

        return personalInfo;
    }

    private static FriendInfo ToFriendInfo(Friend p) => new(p.Id, p.Name, p.Surname, p.Age, p.City, p.Gender, p.Bio, p.Status);
}