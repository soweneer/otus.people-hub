using MediatR;
using PeopleHub.Dal.Infrastructure.Db;

namespace PeopleHub.Lib.BusinessLogic.Person.Create;

public sealed class Handler : IRequestHandler<Request, int?>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public async Task<int?> Handle(Request request, CancellationToken cancellationToken)
    {
        var person = request.Person;
        return await _dbClient.TryGetIntAsync(
            $"INSERT INTO \"{DbClient.PersonsTable}\" (\"Surname\", \"Name\", \"Age\", \"Gender\", \"City\", \"Bio\") " +
            $"VALUES ('{person.Surname}', '{person.Name}', {person.Age}, {person.Gender:D}, '{person.City}', '{person.Bio}') RETURNING \"Id\"");
    }
}
