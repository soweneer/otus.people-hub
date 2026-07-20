using System.Data;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class PostRepository(DbClient dbClient) : IPostRepository
{
    public async Task<Post> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var dataTable = await dbClient.ExecuteDataTableAsync(
            $"select id, text, author_user_id from {DbClient.PostsTable} where id = @id",
            [("id", id)]);

        return dataTable is null || dataTable.Rows.Count == 0
            ? null
            : ExtractPost(dataTable.Rows[0]);
    }

    public async Task<long?> AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        const string query =
            $"INSERT INTO {DbClient.PostsTable} (author_user_id, text) " +
            "VALUES (@authorUserId, @text) RETURNING id";

        var postId = await dbClient.ExecuteScalarAsync(query,
            [
                ("authorUserId", post.AuthorUserId),
                ("text", post.Text)
            ]);

        return postId is null or DBNull
            ? null
            : Convert.ToInt64(postId);
    }

    public async Task<bool> UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        const string query =
            $"UPDATE {DbClient.PostsTable} " +
            "SET text = @text " +
            "WHERE id = @id AND author_user_id = @authorUserId";

        var affectedRows = 0;
        await dbClient.ExecuteCmdAsync(query,
            async cmd => affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken),
            [
                ("text", post.Text),
                ("id", post.Id),
                ("authorUserId", post.AuthorUserId)
            ]);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(long id, long authorUserId, CancellationToken cancellationToken = default)
    {
        const string query =
            $"DELETE FROM {DbClient.PostsTable} " +
            "WHERE id = @id AND author_user_id = @authorUserId";

        var affectedRows = 0;
        await dbClient.ExecuteCmdAsync(query,
            async cmd => affectedRows = await cmd.ExecuteNonQueryAsync(cancellationToken),
            [
                ("id", id),
                ("authorUserId", authorUserId)
            ]);

        return affectedRows > 0;
    }

    private static Post ExtractPost(DataRow row) =>
        Post.Restore(
            Convert.ToInt64(row["id"]),
            Convert.ToInt32(row["author_user_id"]),
            row["text"].ToString()
        );
}
