using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class AccountRepository(DbClient dbClient) : IAccountRepository
{
    public async Task<int?> CreateAsync(string email, string password, int personId)
    {
        var scalar = await dbClient.ExecuteScalarAsync(
            $"INSERT INTO {DbClient.AccountsTable} (email, password, person_id) " +
            $"VALUES (@email, @password, @personId) RETURNING id",
            [
                ("email", email),
                ("password", password),
                ("personId", personId)
            ]);

        return Convert.ToInt32(scalar);
    }
    
    public async Task<bool> ExistsAsync(string email)
    {
        var dbValue = await dbClient.ExecuteScalarAsync(
            $"SELECT 1 FROM {DbClient.AccountsTable} WHERE email = @email", 
            [("email", email)]);

        return dbValue is not null;
    }
    
    public async Task<Account> FindByEmailAsync(string email)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"SELECT * FROM {DbClient.AccountsTable} WHERE email = @email",
            [("email", email)]);
        if (dataTable.Rows.Count == 0)
        {
            return null;
        }

        var dataRow = dataTable.Rows[0];
        return new Account(
            int.Parse(dataRow["id"].ToString()),
            dataRow["email"].ToString(),
            dataRow["password"].ToString(),
             int.Parse(dataRow["person_id"].ToString())
        );
    }
}