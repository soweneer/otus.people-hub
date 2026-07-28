namespace PeopleHub.Infrastructure.Messaging;

public static class FeedTopology
{
    public const string ChangedExchange = "feed.changed";
    public const string PostedExchange = "feed.posted";
    public const string MaterializeQueue = "feed.materialize";
    public const string ChangedBindingKey = "post.#";

    public static string ChangedRoutingKey(FeedChangeType changeType) => $"post.{changeType.ToString().ToLowerInvariant()}";

    public static string UserRoutingKey(long userId) => $"user.{userId}";

    public static bool TryParseUserId(string routingKey, out long userId)
    {
        userId = 0;

        return routingKey is not null
               && routingKey.StartsWith("user.", StringComparison.Ordinal)
               && long.TryParse(routingKey.AsSpan(5), out userId);
    }
}
