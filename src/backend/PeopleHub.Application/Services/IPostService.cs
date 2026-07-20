using PeopleHub.Domain.Entities;

namespace PeopleHub.Application.Services;

public partial interface IPostService
{
    Task<long?> CreateAsync(long userId, string text, CancellationToken cancellationToken = default);

    Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(long userId, long postId, string text, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(long userId, long postId, CancellationToken cancellationToken = default);
}
