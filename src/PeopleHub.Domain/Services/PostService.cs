using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PostService(IPostRepository postRepository, IUserRepository userRepository) : IPostService
{
    public Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default) =>
        postRepository.GetAsync(postId, cancellationToken);

    public async Task<long?> CreateAsync(string authorEmail, string text, CancellationToken cancellationToken = default)
    {
        var authorUserId = await userRepository.GetUserIdAsync(authorEmail, cancellationToken);

        return await postRepository.CreateAsync(authorUserId, text, cancellationToken);
    }

    public async Task<bool> UpdateAsync(string authorEmail, long postId, string text, CancellationToken cancellationToken = default)
    {
        var authorUserId = await userRepository.GetUserIdAsync(authorEmail, cancellationToken);

        return await postRepository.UpdateAsync(postId, authorUserId, text, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string authorEmail, long postId, CancellationToken cancellationToken = default)
    {
        var authorUserId = await userRepository.GetUserIdAsync(authorEmail, cancellationToken);

        return await postRepository.DeleteAsync(postId, authorUserId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Post>> GetFeedAsync(string email, int offset, int limit, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        return await postRepository.GetFriendsFeedAsync(userId, offset, limit, cancellationToken);
    }
}
