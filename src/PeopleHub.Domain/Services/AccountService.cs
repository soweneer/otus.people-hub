using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class AccountService(IPersonRepository personRepository,
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher) : IAccountService
{
    public async Task<bool> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.FindByEmailAsync(email);

        return account is not null && passwordHasher.Verify(account.Password, password);
    }

    public async Task<SignUpStatus> SignUpAsync(string email, string password, PersonalInfo personalInfo,
        CancellationToken cancellationToken = default)
    {
        if (await accountRepository.ExistsAsync(email))
        {
            return SignUpStatus.AlreadyExists;
        }

        var personId = await personRepository.CreateAsync(personalInfo, cancellationToken);
        if (personId.HasValue)
        {
            var accountResult = await accountRepository.CreateAsync(email, passwordHasher.Hash(password), personId.Value);
            if (accountResult.HasValue)
            {
                return SignUpStatus.Success;
            }
        }

        return SignUpStatus.Failed;
    }
}
