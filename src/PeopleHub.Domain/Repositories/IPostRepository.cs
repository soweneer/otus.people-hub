namespace PeopleHub.Domain.Repositories;

public interface IPostRepository
{
    Task<long?> CreateAsync(int authorUserId, string text, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(long id, int authorUserId, string text, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, int authorUserId, CancellationToken cancellationToken);
}
