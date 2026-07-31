using Prometheus;

namespace PeopleHub.Counters.Services;

internal static class CounterMetrics
{
    public static readonly Counter Applied = Metrics.CreateCounter(
        "counters_events_applied_total",
        "Сколько событий счётчиков применено",
        new CounterConfiguration { LabelNames = ["type"] });

    public static readonly Counter Failed = Metrics.CreateCounter(
        "counters_events_failed_total",
        "Сколько событий счётчиков ушло в dead-letter");

    public static readonly Histogram ApplyDuration = Metrics.CreateHistogram(
        "counters_apply_duration_seconds",
        "Время применения одного события к Redis",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.0005, 2, 12) });

    public static readonly Counter Reconciled = Metrics.CreateCounter(
        "counters_reconciled_total",
        "Сколько раз счётчики пользователя пересобирались из базы диалогов");

    public static readonly Counter Drifts = Metrics.CreateCounter(
        "counters_drift_total",
        "Сколько раз сверка нашла расхождение между Redis и базой диалогов");

    public static readonly Counter ColdRebuilds = Metrics.CreateCounter(
        "counters_cold_rebuild_total",
        "Сколько раз счётчики собирались на лету из-за отсутствия данных в Redis");

    public static readonly Histogram ReadDuration = Metrics.CreateHistogram(
        "counters_read_duration_seconds",
        "Время чтения счётчиков из Redis",
        new HistogramConfiguration
        {
            LabelNames = ["method"],
            Buckets = Histogram.ExponentialBuckets(0.0002, 2, 12)
        });
}
