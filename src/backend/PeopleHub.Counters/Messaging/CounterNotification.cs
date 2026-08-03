using System.Text.Json.Serialization;

namespace PeopleHub.Counters.Messaging;

public sealed record CounterNotification(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("partnerId")] string PartnerId,
    [property: JsonPropertyName("count")] long Count,
    [property: JsonPropertyName("total")] long Total)
{
    public const string UnreadKind = "unread";

    public static CounterNotification Unread(long partnerId, long count, long total) =>
        new(UnreadKind, partnerId.ToString(), count, total);
}
