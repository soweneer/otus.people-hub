namespace PeopleHub.Domain.Services;

public interface IPostService
{
    Task<long?> CreateAsync(string authorEmail, string text, CancellationToken cancellationToken = default);
}
