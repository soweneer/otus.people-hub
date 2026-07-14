using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;

namespace PeopleHub.Application.Services;

public interface IPostService
{
    Task<long?> CreateAsync(string text, CancellationToken cancellationToken = default);

    Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(long postId, string text, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long postId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FeedPost>> GetFeedAsync(int offset, int limit, CancellationToken cancellationToken = default);
}
