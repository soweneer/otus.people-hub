using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class FriendRequestRepository(DbClient dbClient) : IFriendRequestRepository
{
    public Task ApproveAsync(int id, int receiverPersonId) =>
        dbClient.ExecuteCmdAsync(
            $"update {DbClient.FriendsRequestsTable} " +
            $"set status = {FriendRequestStatus.Approved:D} " +
            $"where id = {id} and receiver_person_id = {receiverPersonId}",
            cmd => cmd.ExecuteNonQuery());

    public async Task DeleteAsync(int personId, int receiverPersonId)
    {
        var query = $"delete from {DbClient.FriendsRequestsTable} " +
                    "where " +
                    $"(sender_person_id = {personId} and receiver_person_id = {receiverPersonId})" +
                    $" or (sender_person_id = {receiverPersonId} and receiver_person_id = {personId})";

        await dbClient.ExecuteCmdAsync(query, 
            cmd => cmd.ExecuteNonQuery());
    }

    public async Task<FriendsInfo> GetFriendsAsync(int personId)
    {
        var dataSet = await dbClient.GetDataSetASync(
            $"""
             with my_friends as (
                 select * from
                 (
                     select id as request_id, sender_person_id as friend_id, status, 0 as incoming from {DbClient.FriendsRequestsTable} where receiver_person_id = {personId}
                     union all
                     select id as request_id, receiver_person_id as friend_id, status, 1 as incoming from {DbClient.FriendsRequestsTable} where sender_person_id = {personId}
                 )
             )
             select p.*, f.*
             from
                 my_friends f
                 left join {DbClient.PersonsTable} p on f.friend_id = p.id
             """
        );

        var friends = new List<FriendInfoLite>();
        var incoming = new List<FriendInfoLite>();
        var outgoing = new List<FriendInfoLite>();
        foreach (DataRow row in dataSet.Tables[0].Rows)
        {
            var status = Enum.Parse<FriendRequestStatus>(row["status"].ToString());
            var friend = new FriendInfoLite(
                new PersonLite(
                    Convert.ToInt32(row["id"]),
                    $"{row["surname"]} {row["name"]}",
                    Convert.ToInt32(row["age"]),
                    row["city"].ToString()
                ),
                Convert.ToInt32(row["request_id"]));

            if (status is FriendRequestStatus.Approved)
            {
                friends.Add(friend);
            }
            else
            {
                if (bool.TryParse(row["incoming"].ToString(), out var isIncoming) && isIncoming)
                {
                    incoming.Add(friend);
                }
                else
                {
                    outgoing.Add(friend);
                }
            }
        }

        return new FriendsInfo(friends, incoming, outgoing);
    }

    public async Task<FriendRequest> GetAsync(int id)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.FriendsRequestsTable} where id = @id",
            [("id", id)]);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : new FriendRequest(
                Convert.ToInt32(dataTable.Rows[0]["id"]),
                Convert.ToInt32(dataTable.Rows[0]["receiver_person_id"]),
                Convert.ToInt32(dataTable.Rows[0]["sender_person_id"])
            );
    }

    public Task RejectAsync(int id, int receiverPersonId) => 
        dbClient.ExecuteCmdAsync(
            $"update {DbClient.FriendsRequestsTable} " +
            $"set status = {FriendRequestStatus.Rejected:D} " +
            $"where id = {id} and receiver_person_id = {receiverPersonId}",
            cmd => cmd.ExecuteNonQuery()
        );

    public Task SendAsync(int senderPersonId, int receiverPersonId) => 
        dbClient.ExecuteCmdAsync(
            $"insert into {DbClient.FriendsRequestsTable} (sender_person_id, receiver_person_id, status) " +
            $"values ({senderPersonId}, {receiverPersonId}, {FriendRequestStatus.Sent:D})",
            cmd => cmd.ExecuteNonQuery()
        );
}