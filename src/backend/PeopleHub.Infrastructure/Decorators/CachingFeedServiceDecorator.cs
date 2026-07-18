using Microsoft.Extensions.Caching.Distributed;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;

namespace PeopleHub.Infrastructure.Decorators;

public sealed class CachingFeedServiceDecorator(IFeedService underlyingService, IDistributedCache cache) : IFeedService
{
    public async Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(string email, int offset, int limit, CancellationToken cancellationToken = default)
    {
        return await underlyingService.GetFeedAsync(email, offset, limit, cancellationToken);
    }
}
