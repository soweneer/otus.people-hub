using System.Data;
using PeopleHub.Domain.Exceptions;
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
}