using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class FriendRepository(DbClient dbClient) : IFriendRepository
{
    public Task ApproveAsync(int id) =>
        dbClient.ExecuteCmdAsync(
            $"UPDATE \"{DbClient.FriendsRequestsTable}\" " +
            $"SET \"status\" = {FriendRequestStatus.Approved:D} " +
            $"WHERE \"id\" = {id}",
            cmd => cmd.ExecuteNonQuery());

    public async Task DeleteAsync(int personId, int receiverPersonId)
    {
        var query = $"DELETE FROM \"{DbClient.FriendsRequestsTable}\" " +
                    "WHERE " +
                    $"(\"sender_person_id\" = {personId} AND \"receiver_person_id\" = {receiverPersonId})" +
                    $" OR (\"sender_person_id\" = {receiverPersonId} AND \"receiver_person_id\" = {personId})";

        await dbClient.ExecuteCmdAsync(query, 
            cmd => cmd.ExecuteNonQuery());
    }

    public async Task<FriendsInfo> GetFriendsAsync(int personId)
    {
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

        return new FriendsInfo(friends, incomingRequests, outcomingRequests);
    }

    public async Task<FriendRequest> GetAsync(int id)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"SELECT * FROM \"{DbClient.FriendsRequestsTable}\" WHERE \"id\" = @id",
            [("id", id)]);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : new FriendRequest(
                Convert.ToInt32(dataTable.Rows[0]["id"]),
                Convert.ToInt32(dataTable.Rows[0]["receiver_person_id"]),
                Convert.ToInt32(dataTable.Rows[0]["sender_person_id"])
            );
    }

    public Task RejectAsync(int id) => 
        dbClient.ExecuteCmdAsync(
            $"UPDATE \"{DbClient.FriendsRequestsTable}\" " +
            $"SET \"status\" = {FriendRequestStatus.Rejected:D} " +
            $"WHERE \"id\" = {id}", cmd => cmd.ExecuteNonQuery()
        );

    public Task SendAsync(int senderPersonId, int receiverPersonId) => 
        dbClient.ExecuteCmdAsync(
            $"INSERT INTO \"{DbClient.FriendsRequestsTable}\" (\"sender_person_id\", \"receiver_person_id\", \"status\") " +
            $"VALUES ({senderPersonId}, {receiverPersonId}, {FriendRequestStatus.Sent:D})",
            cmd => cmd.ExecuteNonQuery()
        );
    
    private static FriendInfo ParseFriendRequestInfoFromRow(DataRow row, FriendRequestStatus? status = null) => 
        new(
            new PersonLite(
                Convert.ToInt32(row["id"]),
                $"{row["surname"]} {row["name"]}",
                Convert.ToInt32(row["age"]),
                row["city"].ToString()
            ),
            Convert.ToInt32(row["request_id"]),
            status ?? Enum.Parse<FriendRequestStatus>(row["status"].ToString())
        );
}