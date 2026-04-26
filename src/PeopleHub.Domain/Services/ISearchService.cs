using PeopleHub.Domain.Model;

namespace PeopleHub.Domain.Services;

public interface ISearchService
{
    Task<IReadOnlyCollection<PersonInfo>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);
}
