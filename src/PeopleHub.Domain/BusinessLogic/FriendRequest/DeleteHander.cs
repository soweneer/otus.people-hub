using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.BusinessLogic.Person;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record DeleteRequest(string InitiatorEmail, int ReceiverPersonId): IRequest;

public sealed class DeleteHander(DbClient dbClient, IMediator mediator) : IRequestHandler<DeleteRequest>
{
    public async Task Handle(DeleteRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.InitiatorEmail), cancellationToken);

        var query = $"DELETE FROM \"{DbClient.FriendsTable}\" " +
                    "WHERE " +
                    $"(\"sender_person_id\" = {personId} AND \"receiver_person_id\" = {request.ReceiverPersonId})" +
                    $" OR (\"sender_person_id\" = {request.ReceiverPersonId} AND \"receiver_person_id\" = {personId})";

        await dbClient.RunCmdAsync(query);
    }
}
