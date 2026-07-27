using PeopleHub.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PeopleHub.Feed.WebSockets;

public sealed class FeedSubscriber(
    RabbitMqConnection connection,
    FeedConnectionRegistry registry,
    IHostApplicationLifetime lifetime,
    ILogger<FeedSubscriber> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _queueName = $"ws.{Environment.MachineName}.{Guid.NewGuid():N}";
    private IChannel _channel;

    public async Task SubscribeAsync(long userId, FeedConnection feedConnection, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await EnsureChannelAsync(cancellationToken);
            if (registry.Add(userId, feedConnection))
            {
                await channel.QueueBindAsync(
                    _queueName,
                    FeedTopology.PostedExchange,
                    FeedTopology.UserRoutingKey(userId),
                    cancellationToken: cancellationToken);

                logger.LogInformation("Очередь {Queue} подписана на события пользователя {UserId}", _queueName, userId);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task UnsubscribeAsync(long userId, FeedConnection feedConnection, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!registry.Remove(userId, feedConnection) || _channel is not { IsOpen: true })
            {
                return;
            }

            await _channel.QueueUnbindAsync(
                _queueName,
                FeedTopology.PostedExchange,
                FeedTopology.UserRoutingKey(userId),
                cancellationToken: cancellationToken);

            logger.LogInformation("Очередь {Queue} отписана от событий пользователя {UserId}", _queueName, userId);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        var rabbitConnection = await connection.GetAsync(cancellationToken);
        _channel = await rabbitConnection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            FeedTopology.PostedExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            _queueName,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            if (FeedTopology.TryParseUserId(args.RoutingKey, out var userId))
            {
                await registry.BroadcastAsync(userId, args.Body.ToArray(), lifetime.ApplicationStopping);
            }
        };

        await _channel.BasicConsumeAsync(_queueName, autoAck: true, consumer, cancellationToken: cancellationToken);
        logger.LogInformation("Инстанс слушает очередь {Queue}", _queueName);

        foreach (var userId in registry.SubscribedUserIds)
        {
            await _channel.QueueBindAsync(
                _queueName,
                FeedTopology.PostedExchange,
                FeedTopology.UserRoutingKey(userId),
                cancellationToken: cancellationToken);
        }

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
