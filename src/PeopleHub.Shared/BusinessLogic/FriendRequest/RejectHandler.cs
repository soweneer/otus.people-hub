using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Shared.BusinessLogic.FriendRequest;

public sealed record RejectRequest(int Id): IRequest;

public sealed class RejectHandler(DbClient dbClient) : IRequestHandler<RejectRequest>
{
    public Task Handle(RejectRequest request, CancellationToken cancellationToken)
    {
        return dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.FriendsTable}\" " +
            $"SET \"Status\" = {FriendRequestStatus.Rejected:D} " +
            $"WHERE \"Id\" = {request.Id}");
    }
}
