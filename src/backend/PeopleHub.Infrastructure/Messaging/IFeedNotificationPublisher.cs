namespace PeopleHub.Infrastructure.Messaging;

public interface IFeedNotificationPublisher
{
    Task PublishAsync(long userId, FeedPostedNotification notification, CancellationToken cancellationToken = default);
}
