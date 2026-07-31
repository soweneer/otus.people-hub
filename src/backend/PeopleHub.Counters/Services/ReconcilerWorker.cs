using PeopleHub.Counters.Storage;

namespace PeopleHub.Counters.Services;

internal sealed class ReconcilerWorker(
    ICounterStore store,
    CounterReconciler reconciler,
    ReconcilerOptions options,
    ILogger<ReconcilerWorker> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Сверка счётчиков отключена");
            return;
        }

        var interval = TimeSpan.FromSeconds(options.IntervalSeconds);
        logger.LogInformation("Сверка счётчиков запущена с интервалом {Interval}", interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken);
                await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Сверка счётчиков отвалилась, повтор через {Delay}", RetryDelay);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task SweepAsync(CancellationToken stoppingToken)
    {
        var userIds = await store.GetKnownUserIdsAsync(stoppingToken);
        if (userIds.Count == 0)
        {
            return;
        }

        var repaired = 0;
        foreach (var batch in userIds.Chunk(options.BatchSize))
        {
            foreach (var userId in batch)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                if (await reconciler.ReconcileAsync(userId, stoppingToken))
                {
                    repaired++;
                }
            }
        }

        logger.LogInformation("Сверено пользователей: {Checked}, исправлено: {Repaired}", userIds.Count, repaired);
    }
}
