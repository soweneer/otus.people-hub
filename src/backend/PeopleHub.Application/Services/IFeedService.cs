using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFeedService
{
    const int FeedCapacity = 1000;

    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId, CancellationToken cancellationToken = default);
}
