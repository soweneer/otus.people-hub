using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Infrastructure.Caching;

internal sealed class FeedCacheCorrectionWorker(FeedCacheCorrectionQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<FeedCacheCorrectionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var feedChangeEvent in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ApplyAsync(feedChangeEvent, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Не удалось скорректировать кэш ленты по событию {Type} для поста {PostId}",
                    feedChangeEvent.Type, feedChangeEvent.Post.Id);
            }
        }
    }

    private async Task ApplyAsync(FeedChangeEvent feedChangeEvent, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var friendRequestRepository = scope.ServiceProvider.GetRequiredService<IFriendRequestRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<IFeedCacheService>();

        var friendIds = await friendRequestRepository.GetFriendIdsAsync(feedChangeEvent.Post.AuthorUserId, cancellationToken);
        foreach (var friendId in friendIds)
        {
            await (feedChangeEvent.Type switch
            {
                FeedChangeType.Created => cacheService.AddPostAsync(friendId, feedChangeEvent.Post),
                FeedChangeType.Updated => cacheService.UpdatePostAsync(friendId, feedChangeEvent.Post),
                FeedChangeType.Deleted => cacheService.RemovePostAsync(friendId, feedChangeEvent.Post.Id),
                _ => Task.CompletedTask
            });
        }
    }
}
