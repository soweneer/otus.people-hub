using System.Data;
using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Enums;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Queries;

internal sealed class FriendQueries(DbClient dbClient) : IFriendQueries
{
    public async Task<FriendsInfo> GetFriendsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"""
             with my_friends as (
                 select * from
                 (
                     select id as request_id, sender_user_id as friend_id, status, 1 as incoming from {DbClient.FriendsRequestsTable} where receiver_user_id = @userId
                     union all
                     select id as request_id, receiver_user_id as friend_id, status, 0 as incoming from {DbClient.FriendsRequestsTable} where sender_user_id = @userId
                 )
             )
             select u.*, f.*
             from
                 my_friends f
                 left join {DbClient.UsersTable} u on f.friend_id = u.id
             """,
            [("userId", userId)]);

        var friends = new List<FriendInfoLite>();
        var incoming = new List<FriendInfoLite>();
        var outgoing = new List<FriendInfoLite>();
        foreach (DataRow row in dataTable.Rows)
        {
            var status = (FriendRequestStatus)Convert.ToInt32(row["status"]);
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
            else if (Convert.ToInt32(row["incoming"]) == 1)
            {
                incoming.Add(friend);
            }
            else
            {
                outgoing.Add(friend);
            }
        }

        return new FriendsInfo(friends, incoming, outgoing);
    }
}
