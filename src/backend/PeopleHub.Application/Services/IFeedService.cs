using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFeedService
{
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, int offset, int limit, CancellationToken cancellationToken = default);
}
