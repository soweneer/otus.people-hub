using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IUserRepository
{
    Task<int> GetUserIdAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter searchFilter, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserInfo>> SearchFriendsAsync(string currentUserEmail, SearchFilter searchFilter, CancellationToken cancellationToken);
    Task<Friend> GetAsync(int id, int viewerUserId, CancellationToken cancellationToken);
    Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken);
    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken);
    Task UpdateAsync(int id, PersonalInfo personalInfo, CancellationToken cancellationToken);
}
