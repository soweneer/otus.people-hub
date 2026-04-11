using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Exceptions;

namespace PeopleHub.Lib.BusinessLogic.Person.FindByEmail;

public sealed class Handler : IRequestHandler<Request, int>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public async Task<int> Handle(Request request, CancellationToken cancellationToken)
    {
        var query = $"""
            SELECT p."Id"
            FROM
                "Persons" p
                LEFT JOIN "Accounts" a ON a."PersonId" = p."Id"
            WHERE
                a."Email" = '{request.Email}'
            """;
        var dataTable = await _dbClient.GetDataTableAsync(query);

        return dataTable.Rows.Count == 0
            ? throw new UnknownUserException(request.Email)
            : Convert.ToInt32(dataTable.Rows[0]["Id"]);
    }
}
