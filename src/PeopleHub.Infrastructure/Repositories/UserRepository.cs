using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class UserRepository(DbClient dbClient) : IUserRepository
{
    public async Task<int> GetUserIdAsync(string email, CancellationToken cancellationToken)
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

    public async Task<PersonalInfo> GetAsync(int userId, CancellationToken cancellationToken)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.UsersTable} where id = {userId}");

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

    public async Task<int?> CreateAsync(PersonalInfo personalInfo, CancellationToken cancellationToken)
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
                ("bio", bio)
            ]);

        return userId is null or DBNull
            ? null
            : Convert.ToInt32(userId);
    }

    public async Task<IReadOnlyCollection<UserInfo>> SearchAsync(SearchFilter searchFilter, CancellationToken cancellationToken)
    {
        var (firstName, lastName, skip, take) = searchFilter;

        var conditions = new List<string>();
        var parameters = new List<(string, object)>();
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            conditions.Add("u.surname ilike @surname");
            parameters.Add(("surname", $"%{lastName}%"));
        }
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            conditions.Add("u.name ilike @name");
            parameters.Add(("name", $"%{firstName}%"));
        }
        var whereClause = conditions.Count > 0
            ? $"where {string.Join(" and ", conditions)}"
            : string.Empty;

        var selectQuery = $"""
            select u.id, u.surname || ' ' || u.name as name, u.age, u.city as city
            from {DbClient.UsersTable} u
            {whereClause}
            order by u.id
            limit {take} offset {skip};
            """;

        var dataTable = await dbClient.ExecuteDataTableAsync(selectQuery, parameters);
        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        return dataTable.Rows.Cast<DataRow>()
            .Select(row => new UserInfo(
                new UserLite(
                    int.Parse(row["id"].ToString()),
                    row["name"].ToString(),
                    int.Parse(row["age"].ToString()),
                    row["city"].ToString()
                ),
                FriendRequestStatus.None))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<UserInfo>> SearchFriendsAsync(string currentUserEmail, SearchFilter searchFilter, CancellationToken cancellationToken)
    {
        var (firstName, lastName, skip, take) = searchFilter;

        var userId = await GetUserIdAsync(currentUserEmail, cancellationToken);

        var conditions = new List<string>();
        var parameters = new List<(string, object)>();
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            conditions.Add("u.surname like @surname");
            parameters.Add(("surname", lastName + "%"));
        }
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            conditions.Add("u.name like @name");
            parameters.Add(("name", firstName + "%"));
        }
        var whereClause = conditions.Count > 0
            ? $"where {string.Join(" and ", conditions)}"
            : string.Empty;

        var selectQuery = $"""
            with my_friends as (
                select distinct friend_id, status from
                (
                    select sender_user_id as friend_id, status from {DbClient.FriendsRequestsTable} where receiver_user_id = {userId}
                    union all
                    select receiver_user_id as friend_id, status from {DbClient.FriendsRequestsTable} where sender_user_id = {userId}
                )
            )
            select u.id, u.surname || ' ' || u.name as name, u.age, u.city, f.status
            from
                {DbClient.UsersTable} u
                left join my_friends f on friend_id = u.id
            {whereClause}
            order by u.id
            limit {take} offset {skip};
            """;

        var dataTable = await dbClient.ExecuteDataTableAsync(selectQuery, parameters);
        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        return dataTable.Rows.Cast<DataRow>()
            .Select(row => new UserInfo(
                new UserLite(
                    int.Parse(row["id"].ToString()),
                    row["name"].ToString(),
                    int.Parse(row["age"].ToString()),
                    row["city"].ToString()
                    ),
                Convert.IsDBNull(row["status"])
                    ? FriendRequestStatus.None
                    : Enum.Parse<FriendRequestStatus>(row["status"].ToString())))
            .ToArray();
    }

    public async Task<Friend> GetByIdAsync(int userId, int viewerUserId, CancellationToken cancellationToken = default)
    {
        var query = $"""
            select
                    u.*, fr.status
            from
                    {DbClient.UsersTable} u
                    left join {DbClient.FriendsRequestsTable} fr
                        on (u.id = fr.receiver_user_id and fr.sender_user_id = {viewerUserId})
                               or (u.id = fr.sender_user_id and fr.receiver_user_id = {viewerUserId})
            where u.id = {userId};
            """;
        var dataTable = await dbClient.GetDataTableAsync(query, cancellationToken);

        return dataTable.Rows.Count == 0
            ? null
            : Friend.ExtractFromRow(dataTable.Rows[0]);
    }

    public async Task UpdateAsync(int userId, PersonalInfo personalInfo, CancellationToken cancellationToken)
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
                ("bio", bio),
                ("userId", userId)
            ]);
    }
}
