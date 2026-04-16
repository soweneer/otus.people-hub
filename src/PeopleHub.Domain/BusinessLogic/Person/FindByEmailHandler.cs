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
            SELECT p."id"
            FROM
                "{DbClient.PersonsTable}" p
                LEFT JOIN "{DbClient.AccountsTable}" a ON a."person_id" = p."id"
            WHERE
                a."email" = '{request.Email}'
            """;
        var dataTable = await dbClient.GetDataTableAsync(query);

        return dataTable.Rows.Count == 0
            ? throw new UnknownUserException(request.Email)
            : Convert.ToInt32(dataTable.Rows[0]["id"]);
    }
}
