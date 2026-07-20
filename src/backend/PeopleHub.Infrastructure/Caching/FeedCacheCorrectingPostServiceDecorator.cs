using Microsoft.Extensions.Options;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Entities;

namespace PeopleHub.Infrastructure.Caching;

public sealed class FeedCacheCorrectingPostServiceDecorator(IPostService underlyingService, FeedCacheCorrectionQueue queue,
    IOptionsMonitor<FeatureFlagsOptions> featureFlags) : IPostService
{
    private bool CacheEnabled => featureFlags.CurrentValue.UseCacheForFeed;

    public async Task<long?> CreateAsync(long userId, string text, CancellationToken cancellationToken = default)
    {
        var postId = await underlyingService.CreateAsync(userId, text, cancellationToken);
        if (CacheEnabled && postId is not null)
        {
            await queue.PublishAsync(
                new FeedChangeEvent(FeedChangeType.Created, new FeedPost(postId.Value, text, userId)),
                cancellationToken);
        }

        return postId;
    }

    public Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default) =>
        underlyingService.GetAsync(postId, cancellationToken);

    public async Task<bool> UpdateAsync(long userId, long postId, string text, CancellationToken cancellationToken = default)
    {
        var updated = await underlyingService.UpdateAsync(userId, postId, text, cancellationToken);
        if (CacheEnabled && updated)
        {
            await queue.PublishAsync(
                new FeedChangeEvent(FeedChangeType.Updated, new FeedPost(postId, text, userId)),
                cancellationToken);
        }

        return updated;
    }

    public async Task<bool> DeleteAsync(long userId, long postId, CancellationToken cancellationToken = default)
    {
        var deleted = await underlyingService.DeleteAsync(userId, postId, cancellationToken);
        if (CacheEnabled && deleted)
        {
            await queue.PublishAsync(
                new FeedChangeEvent(FeedChangeType.Deleted, new FeedPost(postId, null, userId)),
                cancellationToken);
        }

        return deleted;
    }
}
