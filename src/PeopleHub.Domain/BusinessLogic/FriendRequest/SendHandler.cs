using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.BusinessLogic.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record SendRequest(string SenderPersonEmail, int ReceiverPersonId): IRequest;

public sealed class SendHandler(DbClient dbClient, IMediator mediator) : IRequestHandler<SendRequest>
{
    public async Task Handle(SendRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.SenderPersonEmail), cancellationToken);

        await dbClient.RunCmdAsync(
            $"INSERT INTO \"{DbClient.FriendsTable}\" (\"sender_person_id\", \"receiver_person_id\", \"status\") " +
            $"VALUES ({personId}, {request.ReceiverPersonId}, {FriendRequestStatus.Sent:D})");
    }
}
