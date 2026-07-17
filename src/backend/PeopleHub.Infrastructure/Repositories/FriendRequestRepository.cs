using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class FriendRequestRepository(DbClient dbClient) : IFriendRequestRepository
{
    public async Task<FriendRequest> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.FriendsRequestsTable} where id = @id",
            [("id", id)]);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : ExtractFriendRequest(dataTable.Rows[0]);
    }

    public async Task<FriendRequest> FindBetweenAsync(int userId, int otherUserId, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select * from {DbClient.FriendsRequestsTable} " +
            "where (sender_user_id = @userId and receiver_user_id = @otherUserId) " +
            "or (sender_user_id = @otherUserId and receiver_user_id = @userId)",
            [
                ("userId", userId),
                ("otherUserId", otherUserId)
            ]);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : ExtractFriendRequest(dataTable.Rows[0]);
    }

    public Task AddAsync(FriendRequest request, CancellationToken cancellationToken = default) =>
        dbClient.ExecuteNonQuery(
            $"insert into {DbClient.FriendsRequestsTable} (sender_user_id, receiver_user_id, status) " +
            "values (@senderUserId, @receiverUserId, @status)",
            [
                ("senderUserId", request.SenderUserId),
                ("receiverUserId", request.ReceiverUserId),
                ("status", (int)request.Status)
            ]);

    public Task SaveStatusAsync(FriendRequest request, CancellationToken cancellationToken = default) =>
        dbClient.ExecuteNonQuery(
            $"update {DbClient.FriendsRequestsTable} set status = @status where id = @id",
            [
                ("id", request.Id),
                ("status", (int)request.Status)
            ]);

    public Task DeleteBetweenAsync(int userId, int otherUserId, CancellationToken cancellationToken = default) =>
        dbClient.ExecuteNonQuery(
            $"delete from {DbClient.FriendsRequestsTable} " +
            "where (sender_user_id = @userId and receiver_user_id = @otherUserId) " +
            "or (sender_user_id = @otherUserId and receiver_user_id = @userId)",
            [
                ("userId", userId),
                ("otherUserId", otherUserId)
            ]);

    private static FriendRequest ExtractFriendRequest(DataRow row) =>
        new FriendRequest(
            Convert.ToInt32(row["id"]),
            Convert.ToInt32(row["sender_user_id"]),
            Convert.ToInt32(row["receiver_user_id"]),
            (FriendRequestStatus)Convert.ToInt32(row["status"])
        );
}
