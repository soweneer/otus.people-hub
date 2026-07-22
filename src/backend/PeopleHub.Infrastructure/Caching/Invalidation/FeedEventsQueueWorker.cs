using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Infrastructure.Caching.Invalidation;

internal sealed class FeedEventsQueueWorker(FeedEventsQueue queue, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var feedEvent in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var friendRequestRepository = scope.ServiceProvider.GetRequiredService<IFriendRequestRepository>();
                var cacheService = scope.ServiceProvider.GetRequiredService<IFeedCacheService>();

                var authorUserId = feedEvent.Post.AuthorUserId;
                var friendIds = await friendRequestRepository.GetFriendIdsAsync(authorUserId, stoppingToken);
                foreach (var friendId in friendIds)
                {
                    await (feedEvent.Type switch
                    {
                        FeedChangeType.Created => cacheService.AddPostAsync(friendId, feedEvent.Post),
                        FeedChangeType.Updated => cacheService.UpdatePostAsync(friendId, feedEvent.Post),
                        FeedChangeType.Deleted => cacheService.RemovePostAsync(friendId, feedEvent.Post.Id),
                        _ => Task.CompletedTask
                    });
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
