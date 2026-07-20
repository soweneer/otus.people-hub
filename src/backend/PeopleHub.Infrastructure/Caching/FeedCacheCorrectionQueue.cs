using System.Threading.Channels;

namespace PeopleHub.Infrastructure.Caching;

public sealed class FeedCacheCorrectionQueue
{
    private const int Capacity = 10_000;

    private readonly Channel<FeedChangeEvent> _channel = Channel.CreateBounded<FeedChangeEvent>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });

    public ValueTask PublishAsync(FeedChangeEvent feedChangeEvent, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(feedChangeEvent, cancellationToken);

    public IAsyncEnumerable<FeedChangeEvent> ReadAllAsync(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
