using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Domain.ValueObjects;

namespace PeopleHub.Application.Services;

public class AccountService(IUserRepository userRepository,
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IAccountService
{
    public async Task<int?> CanLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (!Email.TryCreate(email, out var accountEmail))
        {
            return null;
        }

        var account = await accountRepository.FindByEmailAsync(accountEmail, cancellationToken);

        return account is not null && passwordHasher.Verify(account.Password.Value, password)
            ? account.UserId 
            : null;
    }

    public async Task<LoginByIdResult> LoginByUserIdAsync(int userId, string password, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.FindByUserIdAsync(userId, cancellationToken);
        if (account is null)
        {
            return LoginByIdResult.UserNotFound;
        }

        return passwordHasher.Verify(account.Password.Value, password)
            ? LoginByIdResult.Success(account.Email.Value)
            : LoginByIdResult.InvalidPassword;
    }

    public async Task<int?> SignUpAsync(string email, string password, PersonalInfo personalInfo,
        CancellationToken cancellationToken = default)
    {
        if (!Email.TryCreate(email, out var accountEmail) ||
            await accountRepository.ExistsAsync(accountEmail, cancellationToken))
        {
            return null;
        }

        var passwordHash = new PasswordHash(passwordHasher.Hash(password));

        return await unitOfWork.ExecuteAsync(async () =>
        {
            var userId = await userRepository.CreateAsync(User.Create(personalInfo), cancellationToken);
            if (userId is null)
            {
                return null;
            }

            await accountRepository.CreateAsync(Account.Create(accountEmail, passwordHash, userId.Value), cancellationToken);

            return userId;
        }, cancellationToken);
    }
}
