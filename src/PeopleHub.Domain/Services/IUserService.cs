using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInfo>> SearchAsync(string email, SearchFilter filter, CancellationToken cancellationToken = default);

    Task<FriendInfo> GetByEmailAsync(string email, int targetUserId, CancellationToken cancellationToken = default);

    Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default);

    Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default);

    Task<PersonalInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
