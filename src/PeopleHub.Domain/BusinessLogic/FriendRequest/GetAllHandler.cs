using System.Data;
using MediatR;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Domain.BusinessLogic.Person;
using PeopleHub.Domain.Model.Dto.Friend;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.BusinessLogic.FriendRequest;

using FindPersonByEmailRequest = FindByEmailRequest;

public sealed record GetAllRequest(string PersonEmail): IRequest<FriendsInfoDto>;

public sealed class GetAllHandler(IMediator mediator, DbClient dbClient) : IRequestHandler<GetAllRequest, FriendsInfoDto>
{
    public async Task<FriendsInfoDto> Handle(GetAllRequest request, CancellationToken cancellationToken)
    {
        var personId = await mediator.Send(new FindPersonByEmailRequest(request.PersonEmail), cancellationToken);
        
        var dataSet = await dbClient.GetDataSetASync(
            $"""
             SELECT f."id" AS "request_id", f."status", p.* FROM "{DbClient.FriendsRequestsTable}" f LEFT JOIN "{DbClient.PersonsTable}" p ON p."id" = f."sender_person_id" WHERE f."receiver_person_id" = {personId} AND f."status" <> {FriendRequestStatus.Approved:D};
                             SELECT f."id" AS "request_id", f."status", p.* FROM "{DbClient.FriendsRequestsTable}" f LEFT JOIN "{DbClient.PersonsTable}" p ON p."id" = f."receiver_person_id" WHERE f."sender_person_id" = {personId} AND f."status" <> {FriendRequestStatus.Approved:D};
                             SELECT f."id" AS "request_id", p.*
                             FROM
                 	            "{DbClient.FriendsRequestsTable}" f
                                 LEFT JOIN "{DbClient.PersonsTable}" p ON p."id" = f."receiver_person_id"
                             WHERE
                 	            f."sender_person_id" = {personId} AND f."status" = {FriendRequestStatus.Approved:D}
                             UNION ALL
                             SELECT f."id" AS "request_id", p.*
                             FROM
                 	            "{DbClient.FriendsRequestsTable}" f
                                 LEFT JOIN "{DbClient.PersonsTable}" p ON p."id" = f."sender_person_id"
                             WHERE
                 	            f."receiver_person_id" = {personId} AND f."status" = {FriendRequestStatus.Approved:D};
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
                Convert.ToInt32(row["id"]),
                $"{row["surname"]} {row["name"]}",
                Convert.ToInt32(row["age"]),
                row["city"].ToString()
            ),
            Convert.ToInt32(row["request_id"]),
            status ?? Enum.Parse<FriendRequestStatus>(row["status"].ToString())
        );
}
