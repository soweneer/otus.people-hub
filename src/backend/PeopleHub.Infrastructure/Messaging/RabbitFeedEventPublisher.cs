using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace PeopleHub.Infrastructure.Messaging;

public sealed class RabbitFeedEventPublisher(RabbitMqConnection connection, ILogger<RabbitFeedEventPublisher> logger)
    : IFeedEventPublisher, IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel _channel;

    public async Task PublishAsync(FeedEvent feedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var channel = await GetChannelAsync(cancellationToken);
                await channel.BasicPublishAsync(
                    FeedTopology.ChangedExchange,
                    FeedTopology.ChangedRoutingKey(feedEvent.Type),
                    mandatory: false,
                    new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json"
                    },
                    FeedEventSerializer.Serialize(feedEvent),
                    cancellationToken);
            }
            finally
            {
                _gate.Release();
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception,
                "Не удалось опубликовать событие ленты {ChangeType} для поста {PostId}",
                feedEvent.Type, feedEvent.Post.Id);
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        var rabbitConnection = await connection.GetAsync(cancellationToken);
        _channel = await rabbitConnection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        await _channel.ExchangeDeclareAsync(
            FeedTopology.ChangedExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _gate.Dispose();
    }
}
