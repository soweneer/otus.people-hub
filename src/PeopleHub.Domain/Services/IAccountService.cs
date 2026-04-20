using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface IAccountService
{
    Task<bool> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<SignUpStatus> SignUpAsync(string email, string password, PersonalInfo personalInfo, CancellationToken cancellationToken = default);
}
