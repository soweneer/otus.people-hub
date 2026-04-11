using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.BusinessLogic.Person;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record DeleteRequest(string InitiatorEmail, int ReceiverPersonId): IRequest;

public sealed class DeleteHander(DbClient dbClient, IMediator mediator) : IRequestHandler<DeleteRequest>
{
    public async Task Handle(DeleteRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.InitiatorEmail), cancellationToken);

        var query = $"DELETE FROM \"{DbClient.FriendsTable}\" " +
                    "WHERE " +
                    $"(\"SenderPersonId\" = {personId} AND \"ReceiverPersonId\" = {request.ReceiverPersonId})" +
                    $" OR (\"SenderPersonId\" = {request.ReceiverPersonId} AND \"ReceiverPersonId\" = {personId})";
        await dbClient.RunCmdAsync(query);
    }
}
