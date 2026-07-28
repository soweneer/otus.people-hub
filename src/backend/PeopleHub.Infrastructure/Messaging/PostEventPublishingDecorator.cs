using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Entities;

namespace PeopleHub.Infrastructure.Messaging;

public sealed class PostEventPublishingDecorator(IPostService underlyingService, IFeedEventPublisher publisher) : IPostService
{
    public async Task<long?> CreateAsync(long userId, string text, CancellationToken cancellationToken = default)
    {
        var postId = await underlyingService.CreateAsync(userId, text, cancellationToken);
        if (postId is not null)
        {
            await publisher.PublishAsync(
                new FeedEvent(FeedChangeType.Created, new FeedPost(postId.Value, text, userId)),
                cancellationToken);
        }

        return postId;
    }

    public Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default) =>
        underlyingService.GetAsync(postId, cancellationToken);

    public async Task<bool> UpdateAsync(long userId, long postId, string text, CancellationToken cancellationToken = default)
    {
        var updated = await underlyingService.UpdateAsync(userId, postId, text, cancellationToken);
        if (updated)
        {
            await publisher.PublishAsync(
                new FeedEvent(FeedChangeType.Updated, new FeedPost(postId, text, userId)),
                cancellationToken);
        }

        return updated;
    }

    public async Task<bool> DeleteAsync(long userId, long postId, CancellationToken cancellationToken = default)
    {
        var deleted = await underlyingService.DeleteAsync(userId, postId, cancellationToken);
        if (deleted)
        {
            await publisher.PublishAsync(
                new FeedEvent(FeedChangeType.Deleted, new FeedPost(postId, null, userId)),
                cancellationToken);
        }

        return deleted;
    }
}
