using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Person;

namespace PeopleHub.Domain.BusinessLogic.Person;

public sealed record CreateRequest(PersonDto Person): IRequest<int?>;

public sealed class CreateHandler(DbClient dbClient) : IRequestHandler<CreateRequest, int?>
{
    public async Task<int?> Handle(CreateRequest request, CancellationToken cancellationToken)
    {
        var (_, name, surname, age, city, gender, bio, _) = request.Person;

        return await dbClient.TryGetIntAsync(
            $"INSERT INTO \"{DbClient.PersonsTable}\" (\"surname\", \"name\", \"age\", \"gender\", \"city\", \"bio\") " +
            $"VALUES ('{surname}', '{name}', {age}, {gender:D}, '{city}', '{bio}') RETURNING \"Id\"");
    }
}
