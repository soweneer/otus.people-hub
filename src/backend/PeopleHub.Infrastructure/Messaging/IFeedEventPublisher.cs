using PeopleHub.Infrastructure.Caching.Invalidation;

namespace PeopleHub.Infrastructure.Messaging;

public interface IFeedEventPublisher
{
    Task PublishAsync(FeedEvent feedEvent, CancellationToken cancellationToken = default);
}
