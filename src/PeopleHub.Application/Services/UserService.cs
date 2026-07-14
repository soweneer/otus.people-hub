using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class UserService(IUserRepository userRepository,
    IUserQueries userQueries,
    ICurrentUser currentUser) : IUserService
{
    public Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default) =>
        userQueries.SearchAsync(filter, cancellationToken);

    public async Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(SearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var viewerUserId = await currentUser.GetUserIdAsync(cancellationToken);

        return await userQueries.SearchWithFriendStatusAsync(viewerUserId, filter, cancellationToken);
    }

    public async Task<FriendInfo> GetWithFriendStatusAsync(int targetUserId, CancellationToken cancellationToken = default)
    {
        var viewerUserId = await currentUser.GetUserIdAsync(cancellationToken);

        return await userQueries.GetWithFriendStatusAsync(targetUserId, viewerUserId, cancellationToken);
    }

    public Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        userRepository.GetAsync(id, cancellationToken);

    public Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default) =>
        userRepository.CreateAsync(personalInfo, cancellationToken);

    public async Task<PersonalInfo> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        return await userRepository.GetAsync(userId, cancellationToken)
            ?? throw new UnknownUserException(currentUser.Email);
    }

    public async Task<PersonalInfo> UpdateProfileAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        await userRepository.UpdateAsync(userId, personalInfo, cancellationToken);

        return personalInfo;
    }
}
