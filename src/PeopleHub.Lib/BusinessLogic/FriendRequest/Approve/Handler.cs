using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Approve;

public sealed class Handler : IRequestHandler<Request>
{
    private readonly DbClient _dbClient;

    public Handler(DbClient dbClient)
    {
        _dbClient = dbClient;
    }

    public Task Handle(Request request, CancellationToken cancellationToken)
    {
        return _dbClient.RunCmdAsync(
            $"UPDATE \"{DbClient.FriendsTable}\" " +
            $"SET \"Status\" = {FriendRequestStatus.Approved:D} " +
            $"WHERE \"Id\" = {request.Id}");
    }
}
