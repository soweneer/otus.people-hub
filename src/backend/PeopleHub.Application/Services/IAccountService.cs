using PeopleHub.Application.Models;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;

namespace PeopleHub.Application.Services;

public interface IAccountService
{
    Task<bool> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<LoginByIdResult> LoginByUserIdAsync(int userId, string password, CancellationToken cancellationToken = default);

    Task<SignUpStatus> SignUpAsync(string email, string password, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
