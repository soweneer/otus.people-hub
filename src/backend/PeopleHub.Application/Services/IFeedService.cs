using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFeedService
{
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int offset, int limit, CancellationToken cancellationToken = default);
}
