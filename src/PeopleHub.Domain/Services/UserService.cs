using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default) =>
        userRepository.SearchAsync(filter, cancellationToken);

    public Task<IReadOnlyCollection<UserInfo>> SearchAsync(string email, SearchFilter filter, CancellationToken cancellationToken = default) =>
        userRepository.SearchFriendsAsync(email, filter, cancellationToken);

    public async Task<FriendInfo?> GetByEmailAsync(string email, int targetUserId, CancellationToken cancellationToken = default)
    {
        var viewerId = await userRepository.GetUserIdAsync(email, cancellationToken);
        var friend = await userRepository.GetAsync(targetUserId, viewerId, cancellationToken);
        return friend is null
            ? null
            : ToFriendInfo(friend);
    }

    public Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default) =>
        userRepository.GetAsync(id, cancellationToken);

    public async Task<PersonalInfo> GetProfileAsync(string email, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await userRepository.GetAsync(userId, cancellationToken)
            ?? throw new UnknownUserException(email);
    }

    public async Task<PersonalInfo> UpdateAsync(string email, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var currentUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        await userRepository.UpdateAsync(currentUserId, personalInfo, cancellationToken);

        return personalInfo;
    }

    private static FriendInfo ToFriendInfo(Friend p) => new(p.Id, p.Name, p.Surname, p.Age, p.City, p.Gender, p.Bio, p.Status);
}
