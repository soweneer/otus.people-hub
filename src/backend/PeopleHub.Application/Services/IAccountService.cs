using PeopleHub.Application.Models;
using PeopleHub.Domain.Model;

namespace PeopleHub.Application.Services;

public interface IAccountService
{
    Task<long?> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<LoginByIdResult> LoginByUserIdAsync(long userId, string password, CancellationToken cancellationToken = default);

    Task<long?> SignUpAsync(string email, string password, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
