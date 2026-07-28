using System.Text.Encodings.Web;
using System.Text.Json;
using PeopleHub.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace PeopleHub.Feed.Services;

public sealed class FeedNotificationPublisher(RabbitMqConnection connection) : IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel _channel;

    public async Task PublishAsync(long userId, FeedPostedNotification notification, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            await channel.BasicPublishAsync(
                FeedTopology.PostedExchange,
                FeedTopology.UserRoutingKey(userId),
                mandatory: false,
                new BasicProperties { ContentType = "application/json" },
                JsonSerializer.SerializeToUtf8Bytes(notification, SerializerOptions),
                cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
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
