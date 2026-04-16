using System.Data;
using PeopleHub.Domain.Exceptions;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class PersonRepository(DbClient dbClient) : IPersonRepository
{
    public async Task<int> GetPersonIdAsync(string email, CancellationToken cancellationToken)
    {
        const string query = $"""
                              SELECT p."id"
                              FROM
                                  "{DbClient.AccountsTable}" a
                                  LEFT JOIN "{DbClient.PersonsTable}" p ON a."person_id" = p."id"
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

    public async Task<int?> CreateAsync(PersonDto person, CancellationToken cancellationToken)
    {
        var (_, name, surname, age, city, gender, bio, _) = person;

        var scalar = await dbClient.ExecuteScalarAsync(
            $"INSERT INTO \"{DbClient.PersonsTable}\" (\"surname\", \"name\", \"age\", \"gender\", \"city\", \"bio\") " +
            $"VALUES ('{surname}', '{name}', {age}, {gender:D}, '{city}', '{bio}') RETURNING \"Id\"",
            []);

        return scalar is null or DBNull ? null : Convert.ToInt32(scalar);
    }

    public async Task<IReadOnlyCollection<PersonDto>> GetAllWithFriendStatusAsync(
        string currentUserEmail, CancellationToken cancellationToken)
    {
        var personId = await GetPersonIdAsync(currentUserEmail, cancellationToken);

        await dbClient.ExecuteCmdAsync(
            "DROP TABLE IF EXISTS \"my_friends\"",
            cmd => cmd.ExecuteNonQuery());

        var personList = new List<PersonDto>();
        var createQuery = $"""
                CREATE TEMPORARY TABLE "my_friends" AS
                SELECT DISTINCT "friend_id", "status" FROM
                    (SELECT "sender_person_id" AS "friend_id", "status" FROM "{DbClient.FriendsRequestsTable}" WHERE "receiver_person_id" = {personId}
                    UNION ALL
                    SELECT "receiver_person_id" AS "friend_id", "status" FROM "{DbClient.FriendsRequestsTable}" WHERE "sender_person_id" = {personId}) AS TMP
            """;
        await dbClient.ExecuteCmdAsync(createQuery, cmd => cmd.ExecuteNonQuery());

        var selectQuery = $"""
                SELECT p.*, f."status"
                FROM
                    "{DbClient.PersonsTable}" p
                    LEFT JOIN "my_friends" f ON f."friend_id" = p."id"
                WHERE
                    p."id" <> {personId}
            """;
        var dataTable = await dbClient.GetDataTableAsync(selectQuery, cancellationToken);
        if (dataTable == null || dataTable.Rows.Count == 0)
            return personList;
        personList.AddRange(
            from DataRow row in dataTable.Rows
            select PersonDto.ExtractFromRow(row)
        );

        await dbClient.ExecuteCmdAsync(
            "DROP TABLE IF EXISTS \"my_friends\"",
            cmd => cmd.ExecuteNonQuery());

        return personList;
    }

    public async Task<PersonDto> GetByIdAsync(
        int personId, int? currentPersonId, CancellationToken cancellationToken)
    {
        var query = currentPersonId.HasValue
            ? $"SELECT p.*, f.\"Status\" FROM \"{DbClient.PersonsTable}\" p LEFT JOIN \"{DbClient.FriendsRequestsTable}\" f ON " +
              $"(p.\"Id\" = f.\"sender_person_id\" AND f.\"receiver_person_id\" = {currentPersonId.Value}) OR " +
              $"(p.\"Id\" = f.\"receiver_person_id\" AND f.\"sender_person_id\" = {currentPersonId.Value}) " +
              $"WHERE p.\"Id\" = {personId}"
            : $"SELECT *, NULL::INTEGER AS \"Status\" FROM \"{DbClient.PersonsTable}\" WHERE \"Id\" = {personId}";
        var dataTable = await dbClient.GetDataTableAsync(query, cancellationToken);

        return dataTable.Rows.Count == 0
            ? null
            : PersonDto.ExtractFromRow(dataTable.Rows[0]);
    }

    public async Task<PersonDto> UpdateAsync(
        int personId, UpdatePersonDto updateInfo, CancellationToken cancellationToken)
    {
        await dbClient.ExecuteCmdAsync(
            $"UPDATE \"{DbClient.PersonsTable}\" " +
                 $"SET \"surname\" = '{updateInfo.Surname}', \"name\" = '{updateInfo.Name}', \"age\" = {updateInfo.Age}, \"bio\" = '{updateInfo.Bio}', \"city\" = '{updateInfo.City}', \"gender\" = {updateInfo.Gender:D} " +
                 $"WHERE \"Id\" = {personId}",
            cmd => cmd.ExecuteNonQuery());

        return await GetByIdAsync(personId, null, cancellationToken);
    }
}
