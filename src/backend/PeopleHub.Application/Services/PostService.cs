using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class PostService(IPostRepository postRepository,
    IUserRepository userRepository) : IPostService
{
    public Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default) =>
        postRepository.GetAsync(postId, cancellationToken);

    public async Task<long?> CreateAsync(string email, string text, CancellationToken cancellationToken = default)
    {
        var authorUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await postRepository.AddAsync(Post.Create(authorUserId, text), cancellationToken);
    }

    public async Task<bool> UpdateAsync(string email, long postId, string text, CancellationToken cancellationToken = default)
    {
        var editorUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var post = await postRepository.GetAsync(postId, cancellationToken);
        if (post is null || !post.IsAuthoredBy(editorUserId))
        {
            return false;
        }

        post.Edit(editorUserId, text);

        return await postRepository.UpdateAsync(post, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string email, long postId, CancellationToken cancellationToken = default)
    {
        var authorUserId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await postRepository.DeleteAsync(postId, authorUserId, cancellationToken);
    }
}
