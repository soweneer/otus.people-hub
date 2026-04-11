using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Friend;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest;

public sealed record GetRequest(int Id): IRequest<DtoFriendRequest>;

public sealed class GetHandler(DbClient dbClient) : IRequestHandler<GetRequest, DtoFriendRequest>
{
    public async Task<DtoFriendRequest> Handle(GetRequest request, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM \"{DbClient.FriendsTable}\" WHERE \"Id\" = {request.Id}";
        var dataTable = await dbClient.GetDataTableAsync(query);
        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : new DtoFriendRequest
            {
                Id = Convert.ToInt32(dataTable.Rows[0]["Id"]),
                ReceiverPersonId = Convert.ToInt32(dataTable.Rows[0]["ReceiverPersonId"]),
                SenderPersonId = Convert.ToInt32(dataTable.Rows[0]["SenderPersonId"])
            };
    }
}
