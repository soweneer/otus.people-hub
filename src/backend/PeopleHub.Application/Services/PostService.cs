using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class PostService(IPostRepository postRepository) : IPostService
{
    public Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default) =>
        postRepository.GetAsync(postId, cancellationToken);

    public async Task<long?> CreateAsync(long userId, string text, CancellationToken cancellationToken = default) => 
        await postRepository.AddAsync(Post.Create(userId, text), cancellationToken);

    public async Task<bool> UpdateAsync(long userId, long postId, string text, CancellationToken cancellationToken = default)
    {
        var post = await postRepository.GetAsync(postId, cancellationToken);
        if (post is null || !post.IsAuthoredBy(userId))
        {
            return false;
        }

        post.Edit(userId, text);

        return await postRepository.UpdateAsync(post, cancellationToken);
    }

    public async Task<bool> DeleteAsync(long userId, long postId, CancellationToken cancellationToken = default) => 
        await postRepository.DeleteAsync(postId, userId, cancellationToken);
}
