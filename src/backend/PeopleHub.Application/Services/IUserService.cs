using PeopleHub.Application.Models;
using PeopleHub.Domain.Model;

namespace PeopleHub.Application.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(long userId, SearchFilter filter, CancellationToken cancellationToken = default);

    Task<FriendInfo> GetWithFriendStatusAsync(long userId, long targetUserId, CancellationToken cancellationToken = default);

    Task<PersonalInfo?> GetAsync(long id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default);

    Task<PersonalInfo> GetProfileAsync(long userId, CancellationToken cancellationToken = default);

    Task<PersonalInfo> UpdateProfileAsync(long userId, PersonalInfo personalInfo, CancellationToken cancellationToken = default);

}
