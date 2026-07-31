using Prometheus;

namespace PeopleHub.Chats.Outbox;

internal static class OutboxMetrics
{
    public static readonly Counter Published = Metrics.CreateCounter(
        "chats_outbox_published_total",
        "Сколько событий аутбокса ушло в брокер");

    public static readonly Gauge Pending = Metrics.CreateGauge(
        "chats_outbox_pending",
        "Сколько событий аутбокса ждут публикации");

    public static readonly Gauge LagSeconds = Metrics.CreateGauge(
        "chats_outbox_lag_seconds",
        "Возраст самого старого неопубликованного события аутбокса");
}
