using System.Data;
using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Enums;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Queries;

internal sealed class UserQueries(DbClient dbClient) : IUserQueries
{
    public async Task<IReadOnlyCollection<SearchedUser>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default)
    {
        var (firstName, lastName, skip, take) = filter;

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
            select u.id, u.name, u.surname, u.city, u.bio
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
            .Select(row => new SearchedUser(
                Convert.ToInt32(row["id"]),
                row["name"].ToString(),
                row["surname"].ToString(),
                row["city"].ToString(),
                row["bio"].ToString()))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<UserInfo>> SearchWithFriendStatusAsync(int viewerUserId, SearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var (firstName, lastName, skip, take) = filter;

        var conditions = new List<string>();
        var parameters = new List<(string, object)> { ("viewerUserId", viewerUserId) };
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
                    select sender_user_id as friend_id, status from {DbClient.FriendsRequestsTable} where receiver_user_id = @viewerUserId
                    union all
                    select receiver_user_id as friend_id, status from {DbClient.FriendsRequestsTable} where sender_user_id = @viewerUserId
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
                    Convert.ToInt32(row["id"]),
                    row["name"].ToString(),
                    Convert.ToInt32(row["age"]),
                    row["city"].ToString()
                    ),
                ExtractStatus(row)))
            .ToArray();
    }

    public async Task<FriendInfo> GetWithFriendStatusAsync(int userId, int viewerUserId, CancellationToken cancellationToken = default)
    {
        var query = $"""
            select
                    u.*, fr.status
            from
                    {DbClient.UsersTable} u
                    left join {DbClient.FriendsRequestsTable} fr
                        on (u.id = fr.receiver_user_id and fr.sender_user_id = @viewerUserId)
                               or (u.id = fr.sender_user_id and fr.receiver_user_id = @viewerUserId)
            where u.id = @userId;
            """;
        var dataTable = await dbClient.ExecuteDataTableAsync(query,
            [
                ("userId", userId),
                ("viewerUserId", viewerUserId)
            ]);

        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return null;
        }

        var row = dataTable.Rows[0];
        return new FriendInfo(
            Convert.ToInt32(row["id"]),
            row["name"].ToString(),
            row["surname"].ToString(),
            Convert.ToInt32(row["age"]),
            row["city"].ToString(),
            (Gender)Convert.ToInt32(row["gender"]),
            row["bio"].ToString(),
            ExtractStatus(row));
    }

    private static FriendRequestStatus ExtractStatus(DataRow row) =>
        Convert.IsDBNull(row["status"])
            ? FriendRequestStatus.None
            : (FriendRequestStatus)Convert.ToInt32(row["status"]);
}
