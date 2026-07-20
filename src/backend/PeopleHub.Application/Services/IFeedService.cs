using PeopleHub.Application.Models;

namespace PeopleHub.Application.Services;

public interface IFeedService
{
    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int userId, CancellationToken cancellationToken = default);
}
