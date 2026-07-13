using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class PostService(IPostRepository postRepository, IUserRepository userRepository) : IPostService
{
    public async Task<long?> CreateAsync(string authorEmail, string text, CancellationToken cancellationToken = default)
    {
        var authorUserId = await userRepository.GetUserIdAsync(authorEmail, cancellationToken);

        return await postRepository.CreateAsync(authorUserId, text, cancellationToken);
    }
}
