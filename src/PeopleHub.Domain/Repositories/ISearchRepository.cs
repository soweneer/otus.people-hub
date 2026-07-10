using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface ISearchRepository
{
    Task<IReadOnlyCollection<UserInfo>> SearchAsync(SearchFilter searchFilter, CancellationToken cancellationToken);
}
