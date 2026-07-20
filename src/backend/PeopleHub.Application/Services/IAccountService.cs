using PeopleHub.Application.Models;
using PeopleHub.Domain.Model;

namespace PeopleHub.Application.Services;

public interface IAccountService
{
    Task<int?> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<LoginByIdResult> LoginByUserIdAsync(int userId, string password, CancellationToken cancellationToken = default);

    Task<int?> SignUpAsync(string email, string password, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
