using System.Data;
using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Shared.BusinessLogic.Person;
using PeopleHub.Shared.Model.Dto.Friend;
using PeopleHub.Shared.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Shared.BusinessLogic.FriendRequest;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record GetAllRequest(string PersonEmail): IRequest<FriendsInfoDto>;

public sealed class GetAllHandler(IMediator mediator, DbClient dbClient) : IRequestHandler<GetAllRequest, FriendsInfoDto>
{

    public async Task<FriendsInfoDto> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.PersonEmail), cancellationToken);

        var friendsInfo = new FriendsInfoDto();
            var dataSet = await dbClient.GetDataSetASync(
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

    private static FriendDto ParseFriendRequestInfoFromRow(DataRow row, FriendRequestStatus? status = null)
    {
        return new FriendDto
        {
            FriendRequestId = Convert.ToInt32(row["RequestId"]),
            FriendRequestStatus = status ?? Enum.Parse<FriendRequestStatus>(row["Status"].ToString()),
            Person = new PersonLiteDto
            {
                Id = Convert.ToInt32(row["Id"]),
                Name = $"{row["Surname"]} {row["Name"]}",
                Age = Convert.ToInt32(row["Age"]),
                City = row["City"].ToString()
            }
        };
    }
}
