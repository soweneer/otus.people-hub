using PeopleHub.Application.Models;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public sealed class FeedService(IFeedRepository feedRepository) : IFeedService
{
    private const int FeedCapacity = 1000;
    private const int Offset = 0;

    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int userId, CancellationToken cancellationToken = default)
    {
        var posts = await feedRepository.GetFriendsFeedAsync(userId, Offset, FeedCapacity);

        return [
            ..posts.Select(p => new FeedPost(p.Id, p.Text, p.AuthorUserId))
        ];
    }
}
