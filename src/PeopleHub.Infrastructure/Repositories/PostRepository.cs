using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class PostRepository(DbClient dbClient) : IPostRepository
{
    public async Task<Post> GetAsync(long id, CancellationToken cancellationToken)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select id, text, author_user_id from {DbClient.PostsTable} where id = @id",
            [("id", id)]);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : Post.ExtractFromRow(dataTable.Rows[0]);
    }

    public async Task<long?> CreateAsync(int authorUserId, string text, CancellationToken cancellationToken)
    {
        const string query =
            $"INSERT INTO {DbClient.PostsTable} (author_user_id, text) " +
            "VALUES (@authorUserId, @text) RETURNING id";

        var postId = await dbClient.ExecuteScalarAsync(query,
            [
                ("authorUserId", authorUserId),
                ("text", text)
            ]);

        return postId is null or DBNull
            ? null
            : Convert.ToInt64(postId);
    }

    public async Task<bool> UpdateAsync(long id, int authorUserId, string text, CancellationToken cancellationToken)
    {
        const string query =
            $"UPDATE {DbClient.PostsTable} " +
            "SET text = @text " +
            "WHERE id = @id AND author_user_id = @authorUserId";

        var affectedRows = 0;
        await dbClient.ExecuteCmdAsync(query,
            async cmd => affectedRows = await cmd.ExecuteNonQueryAsync(),
            [
                ("text", text),
                ("id", id),
                ("authorUserId", authorUserId)
            ]);

        return affectedRows > 0;
    }

    public async Task<IReadOnlyCollection<Post>> GetFriendsFeedAsync(int userId, int offset, int limit, CancellationToken cancellationToken)
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
            .Select(Post.ExtractFromRow)
            .ToArray();
    }

    public async Task<bool> DeleteAsync(long id, int authorUserId, CancellationToken cancellationToken)
    {
        const string query =
            $"DELETE FROM {DbClient.PostsTable} " +
            "WHERE id = @id AND author_user_id = @authorUserId";

        var affectedRows = 0;
        await dbClient.ExecuteCmdAsync(query,
            async cmd => affectedRows = await cmd.ExecuteNonQueryAsync(),
            [
                ("id", id),
                ("authorUserId", authorUserId)
            ]);

        return affectedRows > 0;
    }
}
