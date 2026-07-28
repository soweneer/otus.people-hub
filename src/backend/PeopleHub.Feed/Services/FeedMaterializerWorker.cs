using PeopleHub.Application.Models;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Caching;
using PeopleHub.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PeopleHub.Feed.Services;

public sealed class FeedMaterializerWorker(
    RabbitMqConnection connection,
    RabbitMqOptions options,
    FeedNotificationPublisher notificationPublisher,
    IServiceScopeFactory scopeFactory,
    ILogger<FeedMaterializerWorker> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Материализатор ленты отвалился, повтор через {Delay}", RetryDelay);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var rabbitConnection = await connection.GetAsync(stoppingToken);
        await using var channel = await rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            FeedTopology.ChangedExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            FeedTopology.MaterializeQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object> { ["x-queue-type"] = "quorum" },
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            FeedTopology.MaterializeQueue,
            FeedTopology.ChangedExchange,
            FeedTopology.ChangedBindingKey,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, options.PrefetchCount, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await MaterializeAsync(FeedEventSerializer.Deserialize(args.Body.Span), stoppingToken);
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Не удалось материализовать событие ленты");
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            FeedTopology.MaterializeQueue,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("Материализатор ленты слушает очередь {Queue}", FeedTopology.MaterializeQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task MaterializeAsync(FeedEvent feedEvent, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var friendRequestRepository = scope.ServiceProvider.GetRequiredService<IFriendRequestRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<IFeedCacheService>();

        var friendIds = await friendRequestRepository.GetFriendIdsAsync(feedEvent.Post.AuthorUserId, cancellationToken);
        foreach (var friendId in friendIds)
        {
            await (feedEvent.Type switch
            {
                FeedChangeType.Created => cacheService.AddPostAsync(friendId, feedEvent.Post),
                FeedChangeType.Updated => cacheService.UpdatePostAsync(friendId, feedEvent.Post),
                FeedChangeType.Deleted => cacheService.RemovePostAsync(friendId, feedEvent.Post.Id),
                _ => Task.CompletedTask
            });

            if (feedEvent.Type == FeedChangeType.Created)
            {
                await notificationPublisher.PublishAsync(friendId, ToNotification(feedEvent.Post), cancellationToken);
            }
        }

        logger.LogInformation(
            "Событие {ChangeType} по посту {PostId} разослано {FriendCount} подписчикам",
            feedEvent.Type, feedEvent.Post.Id, friendIds.Count);
    }

    private static FeedPostedNotification ToNotification(FeedPost post) => new(post.Id.ToString(), post.Text, post.AuthorUserId.ToString());
}
