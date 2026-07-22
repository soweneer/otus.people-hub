using Prometheus;

namespace PeopleHub.Infrastructure.Caching;

public static class FeedCacheMetrics
{
    public static readonly Counter HitsCounter = Metrics.CreateCounter("feed_cache_hits_total", "Количество попаданий в кэш ленты на api/post/feed");
    public static readonly Counter MissCounter = Metrics.CreateCounter("feed_cache_miss_total", "Количество промахов кэша ленты на api/post/feed");

    public static void Publish()
    {
        HitsCounter.IncTo(0);
        MissCounter.IncTo(0);
    }
}
