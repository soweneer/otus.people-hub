using System.Threading.Channels;

namespace PeopleHub.Infrastructure.Caching.Invalidation;

public sealed class FeedEventsQueue
{
    private const int Capacity = 10_000;

    private readonly Channel<FeedEvent> _channel = Channel.CreateBounded<FeedEvent>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });

    public ValueTask PublishAsync(FeedEvent feedEvent, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(feedEvent, cancellationToken);

    public IAsyncEnumerable<FeedEvent> ReadAllAsync(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
