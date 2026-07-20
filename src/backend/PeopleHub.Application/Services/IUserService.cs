using PeopleHub.Application.Models;
using PeopleHub.Domain.Model;

namespace PeopleHub.Application.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(string email, SearchFilter filter, CancellationToken cancellationToken = default);

    Task<FriendInfo> GetWithFriendStatusAsync(string email, int targetUserId, CancellationToken cancellationToken = default);

    Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default);

    Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default);

    Task<PersonalInfo> UpdateProfileAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default);

}
