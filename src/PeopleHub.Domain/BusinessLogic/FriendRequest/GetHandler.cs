using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Friend;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

public sealed record GetRequest(int Id): IRequest<FriendRequestDto>;

public sealed class GetHandler(DbClient dbClient) : IRequestHandler<GetRequest, FriendRequestDto>
{
    public async Task<FriendRequestDto> Handle(GetRequest request, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM \"{DbClient.FriendsTable}\" WHERE \"Id\" = {request.Id}";
        var dataTable = await dbClient.GetDataTableAsync(query);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : new FriendRequestDto(
                Convert.ToInt32(dataTable.Rows[0]["Id"]),
                Convert.ToInt32(dataTable.Rows[0]["ReceiverPersonId"]),
                Convert.ToInt32(dataTable.Rows[0]["SenderPersonId"])
            );
    }
}
