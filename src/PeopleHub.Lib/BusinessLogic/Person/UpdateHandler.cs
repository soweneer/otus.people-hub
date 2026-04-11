using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person;

using GetPersonRequest = GetRequest;

public sealed record UpdateRequest(int PersonId, DtoUpdatePerson UpdateInfo): IRequest<DtoPerson>;

public sealed class UpdateHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<UpdateRequest, DtoPerson>
{
    public async Task<DtoPerson> Handle(UpdateRequest request, CancellationToken cancellationToken)
    {
        var (personId, data) = request;
        await dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.PersonsTable}\" " +
                 $"SET \"Surname\" = '{data.Surname}', \"Name\" = '{data.Name}', \"Age\" = {data.Age}, \"Bio\" = '{data.Bio}', \"City\" = '{data.City}', \"Gender\" = {data.Gender:D} " +
                 $"WHERE \"Id\" = {personId}");
        return await mediator.Send(new GetPersonRequest(personId), cancellationToken);
    }
}
