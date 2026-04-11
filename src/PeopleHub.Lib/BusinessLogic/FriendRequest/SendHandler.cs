using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.BusinessLogic.Person;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record SendRequest(string SenderPersonEmail, int ReceiverPersonId): IRequest;

public sealed class SendHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<SendRequest>
{
    public async Task Handle(SendRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.SenderPersonEmail), cancellationToken);

        await dbClient.RunCmdAsync(
            $"INSERT INTO \"{DbClient.FriendsTable}\" (\"SenderPersonId\", \"ReceiverPersonId\", \"Status\") " +
            $"VALUES ({personId}, {request.ReceiverPersonId}, {FriendRequestStatus.Sent:D})");
    }
}
