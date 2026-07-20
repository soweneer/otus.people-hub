using Prometheus;

namespace PeopleHub.Infrastructure.Caching;

public static class FeedCacheMetrics
{
    public static readonly Counter Hits = Metrics.CreateCounter(
        "feed_cache_hits_total", "Количество попаданий в кэш ленты на api/post/feed");

    public static readonly Counter Misses = Metrics.CreateCounter(
        "feed_cache_misses_total", "Количество промахов кэша ленты на api/post/feed");
}
