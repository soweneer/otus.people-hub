using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Send;

using FindPersonByEmailRequest = Person.FindByEmail.Request;

public sealed class Handler : IRequestHandler<Request>
{
    private readonly DbClient _dbClient;
    private readonly IMediator _mediator;

    public Handler(DbClient dbClient, IMediator mediator)
    {
        _dbClient = dbClient;
        _mediator = mediator;
    }

    public async Task Handle(Request request, CancellationToken cancellationToken)
    {
        var personId = await _mediator.Send(new FindPersonByEmailRequest(request.SenderPersonEmail), cancellationToken);

        await _dbClient.RunCmdAsync(
            $"INSERT INTO \"{DbClient.FriendsTable}\" (\"SenderPersonId\", \"ReceiverPersonId\", \"Status\") " +
            $"VALUES ({personId}, {request.ReceiverPersonId}, {FriendRequestStatus.Sent:D})");
    }
}
