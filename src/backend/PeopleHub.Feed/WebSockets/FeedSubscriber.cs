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
    private const string QueueName = "ws.feed";

    private static readonly string[] SourceExchanges =
        [FeedTopology.PostedExchange, FeedTopology.CountersPushedExchange];

    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel _channel;

    public async Task SubscribeAsync(long userId, FeedConnection feedConnection, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await EnsureChannelAsync(cancellationToken);
            if (registry.Add(userId, feedConnection))
            {
                await BindUserAsync(channel, userId, cancellationToken);

                logger.LogInformation("Очередь {Queue} подписана на события пользователя {UserId}", QueueName, userId);
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

            foreach (var exchange in SourceExchanges)
            {
                await _channel.QueueUnbindAsync(
                    QueueName,
                    exchange,
                    FeedTopology.UserRoutingKey(userId),
                    cancellationToken: cancellationToken);
            }

            logger.LogInformation("Очередь {Queue} отписана от событий пользователя {UserId}", QueueName, userId);
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

        foreach (var exchange in SourceExchanges)
        {
            await _channel.ExchangeDeclareAsync(
                exchange,
                ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);
        }

        await _channel.QueueDeclareAsync(
            QueueName,
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

        await _channel.BasicConsumeAsync(QueueName, autoAck: true, consumer, cancellationToken: cancellationToken);
        logger.LogInformation("Сервис слушает очередь {Queue}", QueueName);

        foreach (var userId in registry.SubscribedUserIds)
        {
            await BindUserAsync(_channel, userId, cancellationToken);
        }

        return _channel;
    }

    private static async Task BindUserAsync(IChannel channel, long userId, CancellationToken cancellationToken)
    {
        foreach (var exchange in SourceExchanges)
        {
            await channel.QueueBindAsync(
                QueueName,
                exchange,
                FeedTopology.UserRoutingKey(userId),
                cancellationToken: cancellationToken);
        }
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
