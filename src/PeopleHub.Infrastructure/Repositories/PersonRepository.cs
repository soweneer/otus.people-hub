using System.Data;
using PeopleHub.Domain.Entities;
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

        return personId is null or DBNull ? null : Convert.ToInt32(personId);
    }

    public async Task<IReadOnlyCollection<Friend>> GetFriendsAsync(
        string currentUserEmail, CancellationToken cancellationToken)
    {
        var personId = await GetPersonIdAsync(currentUserEmail, cancellationToken);

        var personList = new List<Friend>();
        var selectQuery = $"""
            with my_friends as (
                select distinct friend_id, status from
                (
                    select sender_person_id as friend_id, status from {DbClient.FriendsRequestsTable} where receiver_person_id = {personId}
                    union all
                    select receiver_person_id as friend_id, status from {DbClient.FriendsRequestsTable} where sender_person_id = {personId}
                )
            )
            select p.*, f.status
            from
                my_friends f 
                left join {DbClient.PersonsTable} p on f.friend_id = p.id
            """;
        var dataTable = await dbClient.GetDataTableAsync(selectQuery, cancellationToken);
        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        personList.AddRange(
            from DataRow row in dataTable.Rows
            select Friend.ExtractFromRow(row)
        );

        return personList;
    }

    public async Task<Friend> GetByIdAsync(int personId, int? currentPersonId, CancellationToken cancellationToken = default)
    {
        var curPersonId = currentPersonId ?? 0;
        var query = 
              $"select p.*, f.status from {DbClient.PersonsTable} p " +
              $"left join {DbClient.FriendsRequestsTable} f on " +
                  $"(p.id = f.sender_person_id and f.receiver_person_id = {curPersonId}) or " +
                  $"(p.id = f.receiver_person_id and f.sender_person_id = {curPersonId}) " +
              $"where p.id = {personId}";
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
