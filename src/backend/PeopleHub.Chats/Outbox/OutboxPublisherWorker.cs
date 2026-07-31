namespace PeopleHub.Chats.Outbox;

internal sealed class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    OutboxOptions options,
    CountersEventPublisher publisher,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var idleDelay = TimeSpan.FromMilliseconds(options.PollIntervalMs);
        logger.LogInformation("Публикатор аутбокса запущен, батч {BatchSize}", options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await DrainAsync(stoppingToken);
                if (published < options.BatchSize)
                {
                    await Task.Delay(idleDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Публикатор аутбокса отвалился, повтор через {Delay}", RetryDelay);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task<int> DrainAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();

        var published = await dispatcher.DrainAsync(
            (records, cancellationToken) => publisher.PublishAsync(records, cancellationToken),
            options.BatchSize,
            stoppingToken);

        if (published > 0)
        {
            OutboxMetrics.Published.Inc(published);
            logger.LogInformation("Опубликовано событий аутбокса: {Count}", published);
        }

        var (pending, lagSeconds) = await dispatcher.GetPendingStatsAsync(stoppingToken);
        OutboxMetrics.Pending.Set(pending);
        OutboxMetrics.LagSeconds.Set(lagSeconds);

        return published;
    }
}
