using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.Model.Dto.Friend;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

public sealed record GetRequest(int Id): IRequest<FriendRequestDto>;

public sealed class GetHandler(DbClient dbClient) : IRequestHandler<GetRequest, FriendRequestDto>
{
    public async Task<FriendRequestDto> Handle(GetRequest request, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM \"{DbClient.FriendsRequestsTable}\" WHERE \"id\" = {request.Id}";
        var dataTable = await dbClient.GetDataTableAsync(query);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : new FriendRequestDto(
                Convert.ToInt32(dataTable.Rows[0]["id"]),
                Convert.ToInt32(dataTable.Rows[0]["receiver_person_id"]),
                Convert.ToInt32(dataTable.Rows[0]["sender_person_id"])
            );
    }
}
