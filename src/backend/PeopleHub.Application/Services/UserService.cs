using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class UserService(IUserRepository userRepository, IUserQueries userQueries) : IUserService
{
    public Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default) =>
        userQueries.SearchAsync(filter, cancellationToken);

    public async Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(long userId, SearchFilter filter,
        CancellationToken cancellationToken = default) => await userQueries.SearchWithFriendStatusAsync(userId, filter, cancellationToken);

    public async Task<FriendInfo> GetWithFriendStatusAsync(long userId, long targetUserId, CancellationToken cancellationToken = default) => 
        await userQueries.GetWithFriendStatusAsync(targetUserId, userId, cancellationToken);

    public async Task<PersonalInfo?> GetAsync(long id, CancellationToken cancellationToken = default) =>
        (await userRepository.GetAsync(id, cancellationToken))?.PersonalInfo;

    public Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default) =>
        userRepository.CreateAsync(User.Create(personalInfo), cancellationToken);

    public async Task<PersonalInfo> GetProfileAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetAsync(userId, cancellationToken)
            ?? throw new UnknownUserException(userId);

        return user.PersonalInfo;
    }

    public async Task<PersonalInfo> UpdateProfileAsync(long userId, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetAsync(userId, cancellationToken)
            ?? throw new UnknownUserException(userId);

        user.UpdatePersonalInfo(personalInfo);
        await userRepository.UpdateAsync(user, cancellationToken);

        return user.PersonalInfo;
    }
}
