using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Repositories;

public interface ISearchRepository
{
    Task<IReadOnlyCollection<PersonInfo>> SearchAsync(SearchFilter searchFilter, CancellationToken cancellationToken);
}
