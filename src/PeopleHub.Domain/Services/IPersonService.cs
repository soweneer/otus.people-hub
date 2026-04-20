using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IPersonService
{
    Task<IReadOnlyCollection<PersonInfo>> GetFriendsAsync(string email, CancellationToken cancellationToken = default);

    Task<PersonInfo?> GetByIdAsync(string email, int? targetPersonId, CancellationToken cancellationToken = default);

    Task<PersonInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}