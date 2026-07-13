using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Infrastructure.Repositories;

internal class PostRepository(DbClient dbClient) : IPostRepository
{
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
}
