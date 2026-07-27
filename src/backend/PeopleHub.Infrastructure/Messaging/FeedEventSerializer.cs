using System.Text.Json;
using System.Text.Json.Serialization;
using PeopleHub.Infrastructure.Caching.Invalidation;

namespace PeopleHub.Infrastructure.Messaging;

public static class FeedEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static byte[] Serialize(FeedEvent feedEvent) =>
        JsonSerializer.SerializeToUtf8Bytes(feedEvent, Options);

    public static FeedEvent Deserialize(ReadOnlySpan<byte> body) =>
        JsonSerializer.Deserialize<FeedEvent>(body, Options);
}
