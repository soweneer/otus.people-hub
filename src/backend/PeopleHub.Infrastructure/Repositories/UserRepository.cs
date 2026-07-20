using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class UserRepository(DbClient dbClient) : IUserRepository
{
    public async Task<User?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.UsersTable} where id = @id",
            [("id", id)]);

        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return null;
        }

        var dataRow = dataTable.Rows[0];
        return User.Restore(
            Convert.ToInt32(dataRow["id"]),
            new PersonalInfo(
                dataRow["name"].ToString(),
                dataRow["surname"].ToString(),
                int.Parse(dataRow["age"].ToString()),
                dataRow["city"].ToString(),
                dataRow["bio"].ToString(),
                int.Parse(dataRow["gender"].ToString())
            ));
    }

    public async Task<int?> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var (name, surname, age, city, bio, gender) = user.PersonalInfo;

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

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var (name, surname, age, city, bio, gender) = user.PersonalInfo;

        await dbClient.ExecuteCmdAsync(
            $"UPDATE {DbClient.UsersTable} " +
                 "SET surname = @surname, name = @name, age = @age, bio = @bio, city = @city, gender = @gender " +
                 "WHERE id = @userId",
            async cmd => await cmd.ExecuteNonQueryAsync(cancellationToken),
            [
                ("surname", surname),
                ("name", name),
                ("age", age),
                ("gender", gender),
                ("city", city),
                ("bio", (object)bio ?? DBNull.Value),
                ("userId", user.Id)
            ]);
    }
}
