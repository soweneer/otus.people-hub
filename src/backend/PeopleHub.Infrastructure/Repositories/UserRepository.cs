using System.Data;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class UserRepository(DbClient dbClient) : IUserRepository
{
    public async Task<int> GetUserIdAsync(string email, CancellationToken cancellationToken = default)
    {
        const string query =
            $"""
                SELECT u.id
                FROM
                  {DbClient.AccountsTable} a
                  LEFT JOIN {DbClient.UsersTable} u ON a.user_id = u.id
                WHERE
                  a.email = @email
            """;

        var dataTable = new DataTable();
        await dbClient.ExecuteCmdAsync(query,
            async cmd =>
            {
                await using var dataReader = await cmd.ExecuteReaderAsync();
                dataTable.Load(dataReader);
            },
            [("email", email)],
            readOnly: true);

        return dataTable.Rows.Count == 0
            ? throw new UnknownUserException(email)
            : Convert.ToInt32(dataTable.Rows[0]["id"]);
    }

    public async Task<PersonalInfo?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.UsersTable} where id = @id",
            [("id", id)]);

        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return null;
        }

        var dataRow = dataTable.Rows[0];
        return new PersonalInfo(
            dataRow["name"].ToString(),
            dataRow["surname"].ToString(),
            int.Parse(dataRow["age"].ToString()),
            dataRow["city"].ToString(),
            dataRow["bio"].ToString(),
            int.Parse(dataRow["gender"].ToString())
        );
    }

    public async Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var (name, surname, age, city, bio, gender) = personalInfo;

        const string query =
            $"INSERT INTO {DbClient.UsersTable} (surname, name, age, gender, city, bio) " +
            "VALUES (@surname, @name, @age, @gender, @city, @bio) RETURNING id";
        var userId = await dbClient.ExecuteScalarAsync(query,
            [
                ("surname", surname),
                ("name", name),
                ("age", age),
                ("gender", gender),
                ("city", city),
                ("bio", (object)bio ?? DBNull.Value)
            ]);

        return userId is null or DBNull
            ? null
            : Convert.ToInt32(userId);
    }

    public async Task UpdateAsync(int id, PersonalInfo personalInfo, CancellationToken cancellationToken = default)
    {
        var (name, surname, age, city, bio, gender) = personalInfo;

        await dbClient.ExecuteCmdAsync(
            $"UPDATE {DbClient.UsersTable} " +
                 "SET surname = @surname, name = @name, age = @age, bio = @bio, city = @city, gender = @gender " +
                 "WHERE id = @userId",
            async cmd => await cmd.ExecuteNonQueryAsync(),
            [
                ("surname", surname),
                ("name", name),
                ("age", age),
                ("gender", gender),
                ("city", city),
                ("bio", (object)bio ?? DBNull.Value),
                ("userId", id)
            ]);
    }
}
