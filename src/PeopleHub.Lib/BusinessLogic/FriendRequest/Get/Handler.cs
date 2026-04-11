using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Friend;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Get;

public sealed class Handler : IRequestHandler<Request, DtoFriendRequest>
{
    private readonly DbClient _dbClient;
    private readonly IMediator _mediator;

    public Handler(DbClient dbClient, IMediator mediator)
    {
        _dbClient = dbClient;
        _mediator = mediator;
    }

    public async Task<DtoFriendRequest> Handle(Request request, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM \"{DbClient.FriendsTable}\" WHERE \"Id\" = {request.Id}";
        var dataTable = await _dbClient.GetDataTableAsync(query);
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
