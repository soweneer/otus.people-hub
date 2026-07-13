using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class FriendRequestRepository(DbClient dbClient) : IFriendRequestRepository
{
    public Task ApproveAsync(int id, int receiverUserId) =>
        dbClient.ExecuteCmdAsync(
            $"update {DbClient.FriendsRequestsTable} " +
            $"set status = {FriendRequestStatus.Approved:D} " +
            $"where id = {id} and receiver_user_id = {receiverUserId}",
            async cmd => await cmd.ExecuteNonQueryAsync());

    public async Task DeleteAsync(int id, int receiverUserId)
    {
        var query = $"delete from {DbClient.FriendsRequestsTable} " +
                    "where " +
                    $"(sender_user_id = {id} and receiver_user_id = {receiverUserId})" +
                    $" or (sender_user_id = {receiverUserId} and receiver_user_id = {id})";

        await dbClient.ExecuteCmdAsync(query,
            async cmd => await cmd.ExecuteNonQueryAsync());
    }
 
    public async Task<FriendsInfo> GetFriendsAsync(int userId)
    {
        var dataSet = await dbClient.GetDataSetASync(
            $"""
             with my_friends as (
                 select * from
                 (
                     select id as request_id, sender_user_id as friend_id, status, 0 as incoming from {DbClient.FriendsRequestsTable} where receiver_user_id = {userId}
                     union all
                     select id as request_id, receiver_user_id as friend_id, status, 1 as incoming from {DbClient.FriendsRequestsTable} where sender_user_id = {userId}
                 )
             )
             select u.*, f.*
             from
                 my_friends f
                 left join {DbClient.UsersTable} u on f.friend_id = u.id
             """
        );

        var friends = new List<FriendInfoLite>();
        var incoming = new List<FriendInfoLite>();
        var outgoing = new List<FriendInfoLite>();
        foreach (DataRow row in dataSet.Tables[0].Rows)
        {
            var status = Enum.Parse<FriendRequestStatus>(row["status"].ToString());
            var friend = new FriendInfoLite(
                new UserLite(
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
                Convert.ToInt32(dataTable.Rows[0]["receiver_user_id"]),
                Convert.ToInt32(dataTable.Rows[0]["sender_user_id"])
            );
    }

    public Task RejectAsync(int id, int receiverUserId) =>
        dbClient.ExecuteCmdAsync(
            $"update {DbClient.FriendsRequestsTable} " +
            $"set status = {FriendRequestStatus.Rejected:D} " +
            $"where id = {id} and receiver_user_id = {receiverUserId}",
            async cmd => await cmd.ExecuteNonQueryAsync()
        );

    public Task SendAsync(int senderUserId, int receiverUserId) =>
        dbClient.ExecuteCmdAsync(
            $"insert into {DbClient.FriendsRequestsTable} (sender_user_id, receiver_user_id, status) " +
            $"values ({senderUserId}, {receiverUserId}, {FriendRequestStatus.Sent:D})",
            async cmd => await cmd.ExecuteNonQueryAsync()
        );

    public Task SetFriendAsync(int userId, int friendUserId) =>
        dbClient.ExecuteCmdAsync(
            $"""
             with updated as (
                 update {DbClient.FriendsRequestsTable}
                 set status = {FriendRequestStatus.Approved:D}
                 where (sender_user_id = {userId} and receiver_user_id = {friendUserId})
                    or (sender_user_id = {friendUserId} and receiver_user_id = {userId})
                 returning id
             )
             insert into {DbClient.FriendsRequestsTable} (sender_user_id, receiver_user_id, status)
             select {userId}, {friendUserId}, {FriendRequestStatus.Approved:D}
             where not exists (select 1 from updated)
             """,
            async cmd => await cmd.ExecuteNonQueryAsync()
        );
}
