using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

public sealed record ApproveRequest(int Id): IRequest;

public sealed class ApproveHandler(DbClient dbClient) : IRequestHandler<ApproveRequest>
{
    public Task Handle(ApproveRequest request, CancellationToken cancellationToken) =>
        dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.FriendsTable}\" " +
            $"SET \"status\" = {FriendRequestStatus.Approved:D} " +
            $"WHERE \"id\" = {request.Id}");
}
s