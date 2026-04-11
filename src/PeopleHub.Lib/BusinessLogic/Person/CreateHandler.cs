using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person;

public sealed record CreateRequest(DtoPerson Person): IRequest<int?>;

public sealed class CreateHandler(DbClient dbClient) : IRequestHandler<CreateRequest, int?>
{
    public async Task<int?> Handle(CreateRequest request, CancellationToken cancellationToken)
    {
        var person = request.Person;
        return await dbClient.TryGetIntAsync(
            $"INSERT INTO \"{DbClient.PersonsTable}\" (\"Surname\", \"Name\", \"Age\", \"Gender\", \"City\", \"Bio\") " +
            $"VALUES ('{person.Surname}', '{person.Name}', {person.Age}, {person.Gender:D}, '{person.City}', '{person.Bio}') RETURNING \"Id\"");
    }
}
