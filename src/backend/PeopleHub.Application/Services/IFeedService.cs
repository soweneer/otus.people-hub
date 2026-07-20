using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFeedService
{
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(long userId, CancellationToken cancellationToken = default);
}
