using PeopleHub.Counters.Storage;

namespace PeopleHub.Counters.Services;

internal sealed class CounterReconciler(
    ICounterStore store,
    IDialogTruthSource truthSource,
    ReconcilerOptions options,
    ILogger<CounterReconciler> logger)
{
    public async Task<CounterSnapshot> ReadAsync(long userId, CancellationToken cancellationToken = default)
    {
        var snapshot = await store.GetAsync(userId, cancellationToken);
        if (snapshot is not null)
        {
            return snapshot;
        }

        CounterMetrics.ColdRebuilds.Inc();
        logger.LogInformation("Счётчики пользователя {UserId} отсутствуют в Redis, восстанавливаем из сервиса диалогов", userId);

        return await RebuildAsync(userId, cancellationToken);
    }

    public async Task<bool> ReconcileAsync(long userId, CancellationToken cancellationToken = default)
    {
        var snapshot = await store.GetAsync(userId, cancellationToken);
        if (snapshot is null)
        {
            await RebuildAsync(userId, cancellationToken);
            return true;
        }

        var truth = await truthSource.GetCountsAsync(userId, cancellationToken);
        if (!Diverged(snapshot, truth, options.LimitPerPartner))
        {
            return false;
        }

        CounterMetrics.Drifts.Inc();
        logger.LogWarning(
            "Счётчики пользователя {UserId} разошлись с базой диалогов: в Redis {Cached}, в базе {Truth}",
            userId, snapshot.Total, truth.Sum(count => count.Count));

        await RebuildAsync(userId, cancellationToken);

        return true;
    }

    private async Task<CounterSnapshot> RebuildAsync(long userId, CancellationToken cancellationToken)
    {
        var state = await truthSource.GetStateAsync(userId, cancellationToken);
        var total = await store.ResetAsync(userId, state, cancellationToken);

        CounterMetrics.Reconciled.Inc();

        return new CounterSnapshot(
            state.Where(partner => partner.UnreadMessageIds.Count > 0)
                .Select(partner => new PartnerCount(partner.PartnerId, partner.UnreadMessageIds.Count))
                .ToArray(),
            total);
    }

    private static bool Diverged(CounterSnapshot snapshot, IReadOnlyCollection<PartnerCount> truth, int limitPerPartner)
    {
        if (snapshot.Counters.Count(counter => counter.Count > 0) != truth.Count)
        {
            return true;
        }

        return truth.Any(expected =>
        {
            var cached = snapshot.Counters.FirstOrDefault(counter => counter.PartnerId == expected.PartnerId)?.Count;

            return cached != expected.Count && !(expected.Count > limitPerPartner && cached >= limitPerPartner);
        });
    }
}
