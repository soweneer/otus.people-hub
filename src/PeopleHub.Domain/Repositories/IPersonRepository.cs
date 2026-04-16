using PeopleHub.Domain.Model.Dto.Person;

namespace PeopleHub.Domain.Repositories;

public interface IPersonRepository
{
    Task<int> GetPersonIdAsync(string email, CancellationToken cancellationToken);

    Task<int?> CreateAsync(PersonDto person, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PersonDto>> GetAllWithFriendStatusAsync(
        string currentUserEmail, CancellationToken cancellationToken);

    Task<PersonDto> GetByIdAsync(
        int personId, int? currentPersonId, CancellationToken cancellationToken);

    Task<PersonDto> UpdateAsync(
        int personId, UpdatePersonDto updateInfo, CancellationToken cancellationToken);
}
