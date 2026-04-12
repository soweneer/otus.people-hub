using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Shared.BusinessLogic.FriendRequest;

public sealed record ApproveRequest(int Id): IRequest;

public sealed class ApproveHandler(DbClient dbClient) : IRequestHandler<ApproveRequest>
{
    public Task Handle(ApproveRequest request, CancellationToken cancellationToken)
    {
        return dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.FriendsTable}\" " +
            $"SET \"Status\" = {FriendRequestStatus.Approved:D} " +
            $"WHERE \"Id\" = {request.Id}");
    }
}
