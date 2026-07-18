using PeopleHub.Application.Models;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public sealed class FeedService(IFeedRepository feedRepository,
    IUserRepository userRepository) : IFeedService
{
    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, int offset, int limit, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var posts = await feedRepository.GetFriendsFeedAsync(userId, offset, limit, cancellationToken);

        return [
            ..posts.Select(p => new FeedPost(p.Id, p.Text, p.AuthorUserId))
        ];
    }
}
