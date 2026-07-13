using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PostService(IPostRepository postRepository, IUserRepository userRepository) : IPostService
{
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
}
