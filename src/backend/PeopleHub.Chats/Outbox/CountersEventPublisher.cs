using System.Text;
using PeopleHub.Chats.Messaging;
using RabbitMQ.Client;

namespace PeopleHub.Chats.Outbox;

internal sealed class CountersEventPublisher(RabbitMqConnection connection) : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel _channel;

    public async Task PublishAsync(OutboxRecord record, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);

            await channel.BasicPublishAsync(
                CountersTopology.ChangedExchange,
                record.Type,
                mandatory: false,
                new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    MessageId = record.Id.ToString()
                },
                Encoding.UTF8.GetBytes(record.Payload),
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
        _channel = await rabbitConnection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        await _channel.ExchangeDeclareAsync(
            CountersTopology.ChangedExchange,
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
