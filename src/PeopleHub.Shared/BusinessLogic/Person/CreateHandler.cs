using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Shared.Model.Dto.Person;

namespace PeopleHub.Shared.BusinessLogic.Person;

public sealed record CreateRequest(PersonDto Person): IRequest<int?>;

public sealed class CreateHandler(DbClient dbClient) : IRequestHandler<CreateRequest, int?>
{
    public async Task<int?> Handle(CreateRequest request, CancellationToken cancellationToken)
    {
        var (_, name, surname, age, city, gender, bio, _) = request.Person;

        return await dbClient.TryGetIntAsync(
            $"INSERT INTO \"{DbClient.PersonsTable}\" (\"Surname\", \"Name\", \"Age\", \"Gender\", \"City\", \"Bio\") " +
            $"VALUES ('{surname}', '{name}', {age}, {gender:D}, '{city}', '{bio}') RETURNING \"Id\"");
    }
}
