using PeopleHub.Application.Models;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public sealed class FeedService(IFeedRepository feedRepository, IUserRepository userRepository) : IFeedService
{
    private const int FeedCapacity = 1000;
    private const int Offset = 0;

    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, CancellationToken cancellationToken = default)
    {
        var userId = await userRepository.GetUserIdAsync(email, cancellationToken);

        var posts = await feedRepository.GetFriendsFeedAsync(userId, Offset, FeedCapacity, cancellationToken);

        return [
            ..posts.Select(p => new FeedPost(p.Id, p.Text, p.AuthorUserId))
        ];
    }
}
