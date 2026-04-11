using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Person;

namespace PeopleHub.Lib.BusinessLogic.Person.Update;

using GetPersonRequest = Get.Request;

public sealed class Handler : IRequestHandler<Request, DtoPerson>
{
    private readonly DbClient _dbClient;
    private readonly IMediator _mediator;

    public Handler(DbClient dbClient, IMediator mediator)
    {
        _dbClient = dbClient;
        _mediator = mediator;
    }

    public async Task<DtoPerson> Handle(Request request, CancellationToken cancellationToken)
    {
        var (personId, data) = request;
        await _dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.PersonsTable}\" " +
                 $"SET \"Surname\" = '{data.Surname}', \"Name\" = '{data.Name}', \"Age\" = {data.Age}, \"Bio\" = '{data.Bio}', \"City\" = '{data.City}', \"Gender\" = {data.Gender:D} " +
                 $"WHERE \"Id\" = {personId}");
        return await _mediator.Send(new GetPersonRequest(personId), cancellationToken);
    }
}
