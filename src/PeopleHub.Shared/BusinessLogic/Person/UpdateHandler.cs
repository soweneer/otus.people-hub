using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Shared.Model.Dto.Person;

namespace PeopleHub.Shared.BusinessLogic.Person;

using GetPersonRequest = GetRequest;

public sealed record UpdateRequest(int PersonId, UpdatePersonDto UpdateInfo): IRequest<PersonDto>;

public sealed class UpdateHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<UpdateRequest, PersonDto>
{
    public async Task<PersonDto> Handle(UpdateRequest request, CancellationToken cancellationToken)
    {
        var (personId, data) = request;
        await dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.PersonsTable}\" " +
                 $"SET \"Surname\" = '{data.Surname}', \"Name\" = '{data.Name}', \"Age\" = {data.Age}, \"Bio\" = '{data.Bio}', \"City\" = '{data.City}', \"Gender\" = {data.Gender:D} " +
                 $"WHERE \"Id\" = {personId}");
        return await mediator.Send(new GetPersonRequest(personId), cancellationToken);
    }
}
