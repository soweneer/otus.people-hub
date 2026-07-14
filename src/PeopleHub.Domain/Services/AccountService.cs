using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class AccountService(IUserRepository userRepository,
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher) : IAccountService
{
    public async Task<bool> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.FindByEmailAsync(email);

        return account is not null && passwordHasher.Verify(account.Password, password);
    }

    public async Task<LoginByIdResult> LoginByUserIdAsync(int userId, string password, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.FindByUserIdAsync(userId);
        if (account is null)
        {
            return LoginByIdResult.UserNotFound;
        }

        return passwordHasher.Verify(account.Password, password)
            ? LoginByIdResult.Success(account.Email)
            : LoginByIdResult.InvalidPassword;
    }

    public async Task<SignUpStatus> SignUpAsync(string email, string password, PersonalInfo personalInfo,
        CancellationToken cancellationToken = default)
    {
        if (await accountRepository.ExistsAsync(email))
        {
            return SignUpStatus.AlreadyExists;
        }

        var userId = await userRepository.CreateAsync(personalInfo, cancellationToken);
        if (userId.HasValue)
        {
            var accountResult = await accountRepository.CreateAsync(email, passwordHasher.Hash(password), userId.Value);
            if (accountResult.HasValue)
            {
                return SignUpStatus.Success;
            }
        }

        return SignUpStatus.Failed;
    }
}
