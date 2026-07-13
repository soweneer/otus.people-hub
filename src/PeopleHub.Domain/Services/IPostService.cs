using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Services;

public interface IPostService
{
    Task<long?> CreateAsync(string authorEmail, string text, CancellationToken cancellationToken = default);
    Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(string authorEmail, long postId, string text, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string authorEmail, long postId, CancellationToken cancellationToken = default);
}
