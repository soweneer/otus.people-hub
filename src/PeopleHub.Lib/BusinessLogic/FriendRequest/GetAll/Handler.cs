using System.Data;
using MediatR;
using PeopleHub.Dal.Infrastructure.Db;
using PeopleHub.Lib.Model.Dto.Friend;
using PeopleHub.Lib.Model.Dto.Person;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.GetAll;

using FindPersonByEmailRequest = Person.FindByEmail.Request;

public sealed class Handler : IRequestHandler<Request, DtoFriendsInfo>
{
    private readonly DbClient _dbClient;
    private readonly IMediator _mediator;

    public Handler(IMediator mediator, DbClient dbClient)
    {
        _mediator = mediator;
        _dbClient = dbClient;
    }

    public async Task<DtoFriendsInfo> Handle(Request request, CancellationToken cancellationToken)
    {
        var personId = await _mediator.Send(new FindPersonByEmailRequest(request.PersonEmail), cancellationToken);

        var friendsInfo = new DtoFriendsInfo();
            var dataSet = await _dbClient.GetDataSetASync(
        $@"SELECT f.""Id"" AS ""RequestId"", f.""Status"", p.* FROM ""{DbClient.FriendsTable}"" f LEFT JOIN ""{DbClient.PersonsTable}"" p ON p.""Id"" = f.""SenderPersonId"" WHERE f.""ReceiverPersonId"" = {personId} AND f.""Status"" <> {FriendRequestStatus.Approved:D};
                SELECT f.""Id"" AS ""RequestId"", f.""Status"", p.* FROM ""{DbClient.FriendsTable}"" f LEFT JOIN ""{DbClient.PersonsTable}"" p ON p.""Id"" = f.""ReceiverPersonId"" WHERE f.""SenderPersonId"" = {personId} AND f.""Status"" <> {FriendRequestStatus.Approved:D};
                SELECT f.""Id"" AS ""RequestId"", p.*
                FROM
	                ""{DbClient.FriendsTable}"" f
                    LEFT JOIN ""{DbClient.PersonsTable}"" p ON p.""Id"" = f.""ReceiverPersonId""
                WHERE
	                f.""SenderPersonId"" = {personId} AND f.""Status"" = {FriendRequestStatus.Approved:D}
                UNION ALL
                SELECT f.""Id"" AS ""RequestId"", p.*
                FROM
	                ""{DbClient.FriendsTable}"" f
                    LEFT JOIN ""{DbClient.PersonsTable}"" p ON p.""Id"" = f.""SenderPersonId""
                WHERE
	                f.""ReceiverPersonId"" = {personId} AND f.""Status"" = {FriendRequestStatus.Approved:D};"
                );
            var incomingData = dataSet.Tables[0];
            if (incomingData.Rows.Count > 0)
            {
                foreach (DataRow row in incomingData.Rows)
                    friendsInfo.IncomingRequests.Add(ParseFriendRequestInfoFromRow(row));
            }
            var outcomingData = dataSet.Tables[1];
            if (outcomingData.Rows.Count > 0)
            {
                foreach (DataRow row in outcomingData.Rows)
                    friendsInfo.OutgoingRequests.Add(ParseFriendRequestInfoFromRow(row));
            }
            var friendsData = dataSet.Tables[2];
            if (friendsData.Rows.Count == 0) return friendsInfo;

            foreach (DataRow row in friendsData.Rows)
                friendsInfo.Friends.Add(ParseFriendRequestInfoFromRow(row));
            return friendsInfo;
    }

    private static DtoFriend ParseFriendRequestInfoFromRow(DataRow row, FriendRequestStatus? status = null)
    {
        return new DtoFriend
        {
            FriendRequestId = Convert.ToInt32(row["RequestId"]),
            FriendRequestStatus = status ?? Enum.Parse<FriendRequestStatus>(row["Status"].ToString()),
            Person = new DtoPersonLite
            {
                Id = Convert.ToInt32(row["Id"]),
                Name = $"{row["Surname"]} {row["Name"]}",
                Age = Convert.ToInt32(row["Age"]),
                City = row["City"].ToString()
            }
        };
    }
}
