using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class UserService(IUserRepository userRepository,
    IUserQueries userQueries) : IUserService
{
    public Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default) =>
        userQueries.SearchAsync(filter, cancellationToken);

    public async Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(string email, SearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var viewerUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await userQueries.SearchWithFriendStatusAsync(viewerUserId, filter, cancellationToken);
    }

    public async Task<FriendInfo> GetWithFriendStatusAsync(string email, int targetUserId, CancellationToken cancellationToken = default)
    {
        var viewerUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await userQueries.GetWithFriendStatusAsync(targetUserId, viewerUserId, cancellationToken);
    }

    public async Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        (await userRepository.GetAsync(id, cancellationToken))?.PersonalInfo;

    public Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default) =>
        userRepository.CreateAsync(User.Create(personalInfo), cancellationToken);

    public async Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var user = await userRepository.GetAsync(userId, cancellationToken)
            ?? throw new UnknownUserException(email);

        return user.PersonalInfo;
    }

    public async Task<PersonalInfo> UpdateProfileAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var user = await userRepository.GetAsync(userId, cancellationToken)
            ?? throw new UnknownUserException(email);

        user.UpdatePersonalInfo(personalInfo);
        await userRepository.UpdateAsync(user, cancellationToken);

        return user.PersonalInfo;
    }
}
