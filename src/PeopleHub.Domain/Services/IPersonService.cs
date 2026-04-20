using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IPersonService
{
    Task<IReadOnlyCollection<FriendInfo>> GetFriendsAsync(string email, CancellationToken cancellationToken = default);

    Task<FriendInfo?> GetByIdAsync(string email, int? targetPersonId, CancellationToken cancellationToken = default);

    Task<PersonalInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}