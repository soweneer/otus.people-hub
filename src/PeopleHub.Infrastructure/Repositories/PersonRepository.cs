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
                  "{DbClient.AccountsTable}" a
                  LEFT JOIN {DbClient.PersonsTable} p ON a."person_id" = p.id
                WHERE
                  a."email" = '@email'
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
            "VALUES ('@surname', '@name', @age, @gender, '@city', '@bio') RETURNING id";
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

    public async Task<IReadOnlyCollection<Person>> GetFriendsAsync(
        string currentUserEmail, CancellationToken cancellationToken)
    {
        var personId = await GetPersonIdAsync(currentUserEmail, cancellationToken);

        await dbClient.ExecuteCmdAsync(
            "DROP TABLE IF EXISTS my_friends",
            cmd => cmd.ExecuteNonQuery());

        var personList = new List<Person>();
        var createQuery = $"""
                CREATE TEMPORARY TABLE my_friends AS
                SELECT DISTINCT friend_id, status FROM
                    (SELECT sender_person_id AS friend_id, status FROM {DbClient.FriendsRequestsTable} WHERE receiver_person_id = {personId}
                    UNION ALL
                    SELECT receiver_person_id AS friend_id, status FROM {DbClient.FriendsRequestsTable} WHERE sender_person_id = {personId}) AS TMP
            """;
        await dbClient.ExecuteCmdAsync(createQuery, cmd => cmd.ExecuteNonQuery());

        var selectQuery = $"""
                SELECT p.*, f.status
                FROM
                    {DbClient.PersonsTable} p
                    LEFT JOIN my_friends f ON f.friend_id = p.id
                WHERE
                    p.id <> {personId}
            """;
        var dataTable = await dbClient.GetDataTableAsync(selectQuery, cancellationToken);
        if (dataTable == null || dataTable.Rows.Count == 0)
            return personList;
        personList.AddRange(
            from DataRow row in dataTable.Rows
            select Person.ExtractFromRow(row)
        );

        await dbClient.ExecuteCmdAsync(
            "DROP TABLE IF EXISTS my_friends",
            cmd => cmd.ExecuteNonQuery());

        return personList;
    }

    public async Task<Person> GetByIdAsync(int personId, int? currentPersonId, CancellationToken cancellationToken = default)
    {
        var curPersonId = currentPersonId ?? 0;
        var query = 
              $"SELECT p.*, f.status FROM {DbClient.PersonsTable} p " +
              $"LEFT JOIN {DbClient.FriendsRequestsTable} f ON " +
                  $"(p.id = f.sender_person_id AND f.receiver_person_id = {curPersonId}) OR " +
                  $"(p.id = f.receiver_person_id AND f.sender_person_id = {curPersonId}) " +
              $"WHERE p.id = {personId}";
        var dataTable = await dbClient.GetDataTableAsync(query, cancellationToken);

        return dataTable.Rows.Count == 0
            ? null
            : Person.ExtractFromRow(dataTable.Rows[0]);
    }

    public async Task<Person> UpdateAsync(int personId, PersonalInfo personalInfo, CancellationToken cancellationToken)
    {
        var (name, surname, age, city, bio, gender) = personalInfo;
        
        await dbClient.ExecuteCmdAsync(
            $"UPDATE {DbClient.PersonsTable} " +
                 "SET surname = '@surname', name = '@name', age = @age, bio = '@bio', city = '@city', gender = @gender" +
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

        return await GetByIdAsync(personId, null, cancellationToken);
    }
}
