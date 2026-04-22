using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class PersonRepository(DbClient dbClient) : IPersonRepository
{
    public async Task<int> GetPersonIdAsync(string email, CancellationToken cancellationToken)
    {
        const string query = 
            $"""
                SELECT p.id
                FROM
                  {DbClient.AccountsTable} a
                  LEFT JOIN {DbClient.PersonsTable} p ON a.person_id = p.id
                WHERE
                  a.email = @email
            """;

        var dataTable = new DataTable();
        await dbClient.ExecuteCmdAsync(query,
            cmd =>
            {
                var dataReader = cmd.ExecuteReader();
                dataTable.Load(dataReader);
            },
            [("email", email)]);

        return dataTable.Rows.Count == 0
            ? throw new UnknownUserException(email)
            : Convert.ToInt32(dataTable.Rows[0]["id"]);
    }

    public async Task<PersonalInfo> GetAsync(int personId, CancellationToken cancellationToken)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.PersonsTable} where id = {personId}");
        
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
            $"INSERT INTO {DbClient.PersonsTable} (surname, name, age, gender, city, bio) " +
            "VALUES (@surname, @name, @age, @gender, @city, @bio) RETURNING id";
        var personId = await dbClient.ExecuteScalarAsync(query, 
            [
                ("surname", surname),
                ("name", name),
                ("age", age),
                ("gender", gender),
                ("city", city),
                ("bio", bio)
            ]);

        return personId is null or DBNull
            ? null
            : Convert.ToInt32(personId);
    }

    public async Task<IReadOnlyCollection<PersonInfo>> SearchAsync(string currentUserEmail, SearchFilter searchFilter, CancellationToken cancellationToken)
    {
        var (firstName, lastName, skip, take) = searchFilter;

        var personId = await GetPersonIdAsync(currentUserEmail, cancellationToken);

        var conditions = new List<string>();
        var parameters = new List<(string, object)>();
        if (!string.IsNullOrWhiteSpace(lastName))
        {
            conditions.Add("p.surname like @surname");
            parameters.Add(("surname", lastName + "%"));
        }
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            conditions.Add("p.name like @name");
            parameters.Add(("name", firstName + "%"));
        }
        var whereClause = conditions.Count > 0
            ? $"where {string.Join(" and ", conditions)}"
            : string.Empty;

        var selectQuery = $"""
            with my_friends as (
                select distinct friend_id, status from
                (
                    select sender_person_id as friend_id, status from {DbClient.FriendsRequestsTable} where receiver_person_id = {personId}
                    union all
                    select receiver_person_id as friend_id, status from {DbClient.FriendsRequestsTable} where sender_person_id = {personId}
                )
            )
            select p.id, p.surname || ' ' || p.name as name, p.age, p.city, f.status
            from
                {DbClient.PersonsTable} p
                left join my_friends f on friend_id = p.id
            {whereClause}
            order by p.id
            limit {take} offset {skip};
            """;

        var dataTable = await dbClient.ExecuteDataTableAsync(selectQuery, parameters);
        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        return dataTable.Rows.Cast<DataRow>()
            .Select(row => new PersonInfo(
                new PersonLite(
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

    public async Task<Friend> GetByIdAsync(int personId, int viewerPersonId, CancellationToken cancellationToken = default)
    {
        var query = $"""
            select
                    p.*, fr.status
            from
                    {DbClient.PersonsTable} p
                    left join {DbClient.FriendsRequestsTable} fr 
                        on (p.id = fr.receiver_person_id and fr.sender_person_id = {viewerPersonId})
                               or (p.id = fr.sender_person_id and fr.receiver_person_id = {viewerPersonId})
            where p.id = {personId};
            """;
        var dataTable = await dbClient.GetDataTableAsync(query, cancellationToken);

        return dataTable.Rows.Count == 0
            ? null
            : Friend.ExtractFromRow(dataTable.Rows[0]);
    }

    public async Task UpdateAsync(int personId, PersonalInfo personalInfo, CancellationToken cancellationToken)
    {
        var (name, surname, age, city, bio, gender) = personalInfo;
        
        await dbClient.ExecuteCmdAsync(
            $"UPDATE {DbClient.PersonsTable} " +
                 "SET surname = @surname, name = @name, age = @age, bio = @bio, city = @city, gender = @gender" +
                 "WHERE id = @personId",
            cmd => cmd.ExecuteNonQuery(),
            [
                ("surname", surname),
                ("name", name),
                ("age", age),
                ("gender", gender),
                ("city", city),
                ("bio", bio),
                ("personId", personId)
            ]);
    }
}
