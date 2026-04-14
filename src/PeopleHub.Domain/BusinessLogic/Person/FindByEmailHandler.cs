using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.BusinessLogic.Person;

public sealed record FindByEmailRequest(string Email): IRequest<int>;

public sealed class FindByEmailHandler(DbClient dbClient) : IRequestHandler<FindByEmailRequest, int>
{
    public async Task<int> Handle(FindByEmailRequest request, CancellationToken cancellationToken)
    {
        var query = $"""
            SELECT p."Id"
            FROM
                "Persons" p
                LEFT JOIN "Accounts" a ON a."PersonId" = p."Id"
            WHERE
                a."Email" = '{request.Email}'
            """;
        var dataTable = await dbClient.GetDataTableAsync(query);

        return dataTable.Rows.Count == 0
            ? throw new UnknownUserException(request.Email)
            : Convert.ToInt32(dataTable.Rows[0]["Id"]);
    }
}
