using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface IUserRepository
{
    Task<int> GetUserIdAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserInfo>> SearchAsync(string currentUserEmail, SearchFilter searchFilter, CancellationToken cancellationToken);
    Task<Friend> GetByIdAsync(int userId, int viewerUserId, CancellationToken cancellationToken);
    Task<PersonalInfo> GetAsync(int userId, CancellationToken cancellationToken);
    Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken);
    Task UpdateAsync(int userId, PersonalInfo personalInfo, CancellationToken cancellationToken);
}
