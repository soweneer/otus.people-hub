using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;

namespace PeopleHub.Application.Services;

public partial interface IPostService
{
    Task<long?> CreateAsync(string email, string text, CancellationToken cancellationToken = default);

    Task<Post> GetAsync(long postId, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(string email, long postId, string text, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string email, long postId, CancellationToken cancellationToken = default);
}
