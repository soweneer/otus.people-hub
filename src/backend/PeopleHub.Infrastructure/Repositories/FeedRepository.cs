using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal sealed class FeedRepository(DbClient dbClient) : IFeedRepository
{
    public async Task<IReadOnlyCollection<Post>> GetFriendsFeedAsync(int userId, int offset, int limit,
        CancellationToken cancellationToken = default)
    {
        var query = $"""
                     select p.id, p.text, p.author_user_id
                     from {DbClient.PostsTable} p
                     where p.author_user_id in (
                         select case
                                 when fr.sender_user_id = @userId then fr.receiver_user_id
                                 else fr.sender_user_id
                             end
                         from {DbClient.FriendsRequestsTable} fr
                         where (fr.sender_user_id = @userId or fr.receiver_user_id = @userId)
                             and fr.status = @approvedStatus
                     )
                     order by p.id desc
                     limit {limit} offset {offset};
                     """;

        var dataTable = await dbClient.ExecuteDataTableAsync(query,
        [
            ("userId", userId),
            ("approvedStatus", (int)FriendRequestStatus.Approved)
        ]);

        if (dataTable is null || dataTable.Rows.Count == 0)
        {
            return [];
        }

        return dataTable.Rows.Cast<DataRow>()
            .Select(row => new Post(
                Convert.ToInt64(row["id"]),
                row["text"].ToString(),
                Convert.ToInt32(row["author_user_id"])))
            .ToArray();
    }
}