using PeopleHub.Application.Models;
using PeopleHub.Domain.Model;

namespace PeopleHub.Application.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<FriendInfo> GetWithFriendStatusAsync(int targetUserId, CancellationToken cancellationToken = default);

    Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default);

    Task<PersonalInfo> GetProfileAsync(CancellationToken cancellationToken = default);

    Task<PersonalInfo> UpdateProfileAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
