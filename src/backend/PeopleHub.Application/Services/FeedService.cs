using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public sealed class FeedService(IFeedRepository feedRepository, ICurrentUser currentUser) : IFeedService
{
    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        var userId = await currentUser.GetUserIdAsync(cancellationToken);

        var posts = await feedRepository.GetFriendsFeedAsync(userId, offset, limit, cancellationToken);
        
        return [
            ..posts.Select(p => new FeedPost(p.Id, p.Text, p.AuthorUserId))
        ];
    }
}