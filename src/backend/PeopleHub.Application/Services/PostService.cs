using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class PostService(IPostRepository postRepository,
    IPostQueries postQueries,
    ICurrentUser currentUser) : IPostService
{
    public Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default) =>
        postRepository.GetAsync(postId, cancellationToken);

    public async Task<long?> CreateAsync(string text, CancellationToken cancellationToken = default)
    {
        var authorUserId = await currentUser.GetUserIdAsync(cancellationToken);

        return await postRepository.AddAsync(Post.Create(authorUserId, text), cancellationToken);
    }

    public async Task<bool> UpdateAsync(long postId, string text, CancellationToken cancellationToken = default)
    {
        var editorUserId = await currentUser.GetUserIdAsync(cancellationToken);

        var post = await postRepository.GetAsync(postId, cancellationToken);
        if (post is null || !post.IsAuthoredBy(editorUserId))
        {
            return false;
        }

        post.Edit(editorUserId, text);

        return await postRepository.UpdateAsync(post, cancellationToken);
    }

    public async Task<bool> DeleteAsync(long postId, CancellationToken cancellationToken = default)
    {
        var authorUserId = await currentUser.GetUserIdAsync(cancellationToken);

        return await postRepository.DeleteAsync(postId, authorUserId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        return await postQueries.GetFriendsFeedAsync(userId, offset, limit, cancellationToken);
    }
}
