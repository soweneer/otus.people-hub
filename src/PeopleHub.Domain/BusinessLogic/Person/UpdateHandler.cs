using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Person;

namespace PeopleHub.Domain.BusinessLogic.Person;

using GetPersonRequest = GetRequest;

public sealed record UpdateRequest(int PersonId, UpdatePersonDto UpdateInfo): IRequest<PersonDto>;

public sealed class UpdateHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<UpdateRequest, PersonDto>
{
    public async Task<PersonDto> Handle(UpdateRequest request, CancellationToken cancellationToken)
    {
        var (personId, data) = request;

        await dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.PersonsTable}\" " +
                 $"SET \"surname\" = '{data.Surname}', \"name\" = '{data.Name}', \"age\" = {data.Age}, \"bio\" = '{data.Bio}', \"city\" = '{data.City}', \"gender\" = {data.Gender:D} " +
                 $"WHERE \"Id\" = {personId}");

        return await mediator.Send(new GetPersonRequest(personId), cancellationToken);
    }
}
