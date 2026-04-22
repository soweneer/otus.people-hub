using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IPersonService
{
    Task<IReadOnlyCollection<PersonInfo>> SearchAsync(string email, SearchFilter filter, CancellationToken cancellationToken = default);

    Task<FriendInfo> GetByEmailAsync(string email, int targetPersonId, CancellationToken cancellationToken = default);

    Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default);

    Task<PersonalInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
