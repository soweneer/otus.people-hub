using MediatR;
using PeopleHub.Dal.Infrastructure.Db;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Delete;

using FindPersonByEmailRequest = Person.FindByEmail.Request;

public sealed class Hander : IRequestHandler<Request>
{
    private readonly IMediator _mediator;
    private readonly DbClient _dbClient;

    public Hander(DbClient dbClient, IMediator mediator)
    {
        _dbClient = dbClient;
        _mediator = mediator;
    }

    public async Task Handle(Request request, CancellationToken cancellationToken)
    {
        var personId = await _mediator.Send(new FindPersonByEmailRequest(request.InitiatorEmail), cancellationToken);

        var query = $"DELETE FROM \"{DbClient.FriendsTable}\" " +
                    "WHERE " +
                    $"(\"SenderPersonId\" = {personId} AND \"ReceiverPersonId\" = {request.ReceiverPersonId})" +
                    $" OR (\"SenderPersonId\" = {request.ReceiverPersonId} AND \"ReceiverPersonId\" = {personId})";
        await _dbClient.RunCmdAsync(query);
    }
}
