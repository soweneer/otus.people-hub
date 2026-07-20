using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.ValueObjects;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class AccountRepository(DbClient dbClient) : IAccountRepository
{
    public async Task<long> CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        var scalar = await dbClient.ExecuteScalarAsync(
            $"INSERT INTO {DbClient.AccountsTable} (email, password, user_id) " +
            "VALUES (@email, @password, @userId) RETURNING id",
            [
                ("email", account.Email.Value),
                ("password", account.Password.Value),
                ("userId", account.UserId)
            ]);

        return Convert.ToInt64(scalar);
    }

    public async Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        var dbValue = await dbClient.ExecuteScalarAsync(
            $"SELECT 1 FROM {DbClient.AccountsTable} WHERE email = @email",
            [("email", email.Value)],
            readOnly: true);

        return dbValue is not null;
    }

    public async Task<Account> FindByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"SELECT * FROM {DbClient.AccountsTable} WHERE email = @email",
            [("email", email.Value)]);

        return ExtractAccount(dataTable);
    }

    public async Task<Account> FindByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"SELECT * FROM {DbClient.AccountsTable} WHERE user_id = @userId",
            [("userId", userId)]);

        return ExtractAccount(dataTable);
    }

    private static Account ExtractAccount(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
        {
            return null;
        }

        var dataRow = dataTable.Rows[0];
        return new Account(
            Convert.ToInt64(dataRow["id"]),
            Email.Create(dataRow["email"].ToString()),
            new PasswordHash(dataRow["password"].ToString()),
            Convert.ToInt64(dataRow["user_id"])
        );
    }
}
