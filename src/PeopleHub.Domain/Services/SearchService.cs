using PeopleHub.Domain.Model;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Domain.Services;

public class SearchService(ISearchRepository searchRepository) : ISearchService
{
    public Task<IReadOnlyCollection<PersonInfo>> SearchAsync(SearchFilter filter,
        CancellationToken cancellationToken = default) =>
        searchRepository.SearchAsync(filter, cancellationToken);
}
