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
        
        var dataSet = await dbClient.GetDataSetASync(
            $"""
             SELECT f."Id" AS "RequestId", f."Status", p.* FROM "{DbClient.FriendsTable}" f LEFT JOIN "{DbClient.PersonsTable}" p ON p."Id" = f."SenderPersonId" WHERE f."ReceiverPersonId" = {personId} AND f."Status" <> {FriendRequestStatus.Approved:D};
                             SELECT f."Id" AS "RequestId", f."Status", p.* FROM "{DbClient.FriendsTable}" f LEFT JOIN "{DbClient.PersonsTable}" p ON p."Id" = f."ReceiverPersonId" WHERE f."SenderPersonId" = {personId} AND f."Status" <> {FriendRequestStatus.Approved:D};
                             SELECT f."Id" AS "RequestId", p.*
                             FROM
                 	            "{DbClient.FriendsTable}" f
                                 LEFT JOIN "{DbClient.PersonsTable}" p ON p."Id" = f."ReceiverPersonId"
                             WHERE
                 	            f."SenderPersonId" = {personId} AND f."Status" = {FriendRequestStatus.Approved:D}
                             UNION ALL
                             SELECT f."Id" AS "RequestId", p.*
                             FROM
                 	            "{DbClient.FriendsTable}" f
                                 LEFT JOIN "{DbClient.PersonsTable}" p ON p."Id" = f."SenderPersonId"
                             WHERE
                 	            f."ReceiverPersonId" = {personId} AND f."Status" = {FriendRequestStatus.Approved:D};
             """
        );

        var incomingData = dataSet.Tables[0];
        var incomingRequests = incomingData.Rows.Count > 0
            ? incomingData.Rows.Cast<DataRow>().Select(r => ParseFriendRequestInfoFromRow(r)).ToArray()
            : [];

        var outcomingData = dataSet.Tables[1];
        var outcomingRequests = outcomingData.Rows.Count > 0
            ? incomingData.Rows.Cast<DataRow>().Select(r => ParseFriendRequestInfoFromRow(r)).ToArray()
            : [];

        var friendsData = dataSet.Tables[2];
        var friends = friendsData.Rows.Count > 0
            ? friendsData.Rows.Cast<DataRow>().Select(r => ParseFriendRequestInfoFromRow(r)).ToArray()
            : [];

        return new FriendsInfoDto(friends, incomingRequests, outcomingRequests);
    }

    private static FriendDto ParseFriendRequestInfoFromRow(DataRow row, FriendRequestStatus? status = null) => 
        new(
            new PersonLiteDto(
                Convert.ToInt32(row["Id"]),
                $"{row["Surname"]} {row["Name"]}",
                Convert.ToInt32(row["Age"]),
                row["City"].ToString()
            ),
            Convert.ToInt32(row["RequestId"]),
            status ?? Enum.Parse<FriendRequestStatus>(row["Status"].ToString())
        );
}
