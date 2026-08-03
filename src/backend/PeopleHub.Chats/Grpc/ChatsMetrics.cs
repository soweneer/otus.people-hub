using Prometheus;

namespace PeopleHub.Chats.Grpc;

internal static class ChatsMetrics
{
    public static readonly Counter Requests = Metrics.CreateCounter(
        "chats_requests_total",
        "Сколько запросов обработал сервис диалогов",
        new CounterConfiguration { LabelNames = ["method", "status"] });

    public static readonly Histogram Duration = Metrics.CreateHistogram(
        "chats_request_duration_seconds",
        "Время обработки запроса сервисом диалогов",
        new HistogramConfiguration
        {
            LabelNames = ["method"],
            Buckets = Histogram.ExponentialBuckets(0.0005, 2, 14)
        });

    public static readonly Gauge InFlight = Metrics.CreateGauge(
        "chats_requests_in_flight",
        "Сколько запросов сервис диалогов обрабатывает прямо сейчас",
        new GaugeConfiguration { LabelNames = ["method"] });
}
