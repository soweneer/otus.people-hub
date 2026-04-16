using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class AccountRepository(DbClient dbClient) : IAccountRepository
{
    public async Task<int?> CreateAsync(string email, string password, int personId)
    {
        var scalar = await dbClient.ExecuteScalarAsync(
            $"INSERT INTO \"{DbClient.AccountsTable}\" (\"email\", \"password\", \"person_id\") " +
            $"VALUES ('@email', '@password', @personId) RETURNING \"id\"",
            [("email", email), ("password", password), ("personId", personId)]);

        return Convert.ToInt32(scalar);
    }
    
    public async Task<bool> ExistsAsync(string email)
    {
        var scalar = await dbClient.ExecuteScalarAsync(
            $"SELECT 1 FROM \"{DbClient.AccountsTable}\" WHERE \"email\" = '@email'", 
            [("email", email)]);

        return scalar is not null;
    }
    
    public async Task<Account> FindByEmailAsync(string email)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"SELECT * FROM \"{DbClient.AccountsTable}\" WHERE \"email\" = '@email'",
            [("email", email)]);

        return dataTable.Rows.Count == 0
            ? null
            : new Account(dataTable.Rows[0]["email"].ToString(), dataTable.Rows[0]["password"].ToString());
    }
}