using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

public sealed record RejectRequest(int Id): IRequest;

public sealed class RejectHandler(DbClient dbClient) : IRequestHandler<RejectRequest>
{
    public Task Handle(RejectRequest request, CancellationToken cancellationToken) => 
        dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.FriendsTable}\" " +
            $"SET \"status\" = {FriendRequestStatus.Rejected:D} " +
            $"WHERE \"id\" = {request.Id}");
}
