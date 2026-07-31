using System.Text.Json;
using PeopleHub.Counters.Messaging;
using PeopleHub.Counters.Storage;
using Prometheus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PeopleHub.Counters.Services;

internal sealed class CounterApplyWorker(
    RabbitMqConnection connection,
    RabbitMqOptions options,
    ICounterStore store,
    ILogger<CounterApplyWorker> logger) : BackgroundService
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
                logger.LogError(exception, "Консьюмер счётчиков отвалился, повтор через {Delay}", RetryDelay);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var rabbitConnection = await connection.GetAsync(stoppingToken);
        await using var channel = await rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);

        await DeclareTopologyAsync(channel, stoppingToken);
        await channel.BasicQosAsync(0, options.PrefetchCount, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await ApplyAsync(args, stoppingToken);
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                CounterMetrics.Failed.Inc();
                logger.LogError(exception, "Не удалось применить событие {RoutingKey}", args.RoutingKey);
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            CountersTopology.ApplyQueue,
            autoAck: false,
            consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("Сервис счётчиков слушает очередь {Queue}", CountersTopology.ApplyQueue);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            CountersTopology.ChangedExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            CountersTopology.DeadExchange,
            ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            CountersTopology.DeadQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object> { ["x-queue-type"] = "quorum" },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            CountersTopology.DeadQueue,
            CountersTopology.DeadExchange,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            CountersTopology.ApplyQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = CountersTopology.DeadExchange
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            CountersTopology.ApplyQueue,
            CountersTopology.ChangedExchange,
            CountersTopology.ApplyBindingKey,
            cancellationToken: cancellationToken);
    }

    private async Task ApplyAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        using var timer = CounterMetrics.ApplyDuration.NewTimer();

        switch (args.RoutingKey)
        {
            case CountersTopology.MessageSentKey:
            {
                var payload = JsonSerializer.Deserialize<MessageSentEvent>(args.Body.Span, JsonSerializerOptions.Web);
                if (payload.FromUserId == payload.ToUserId)
                {
                    return;
                }

                var total = await store.ApplyMessageAsync(
                    payload.ToUserId, payload.FromUserId, payload.MessageId, cancellationToken);

                CounterMetrics.Applied.WithLabels(CountersTopology.MessageSentKey).Inc();
                logger.LogDebug(
                    "Сообщение {MessageId} учтено пользователю {UserId}, всего непрочитанных {Total}",
                    payload.MessageId, payload.ToUserId, total);

                return;
            }

            case CountersTopology.DialogReadKey:
            {
                var payload = JsonSerializer.Deserialize<DialogReadEvent>(args.Body.Span, JsonSerializerOptions.Web);

                var total = await store.ApplyReadAsync(
                    payload.UserId, payload.PartnerId, payload.LastReadMessageId, cancellationToken);

                CounterMetrics.Applied.WithLabels(CountersTopology.DialogReadKey).Inc();
                logger.LogDebug(
                    "Диалог с {PartnerId} прочитан пользователем {UserId} до {MessageId}, всего непрочитанных {Total}",
                    payload.PartnerId, payload.UserId, payload.LastReadMessageId, total);

                return;
            }

            default:
                logger.LogWarning("Неизвестный тип события счётчиков: {RoutingKey}", args.RoutingKey);
                return;
        }
    }
}
