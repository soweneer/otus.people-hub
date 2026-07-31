using PeopleHub.Model;

namespace PeopleHub.Counters;

public interface ICounterService
{
    Task<UnreadCountersResponse> GetUnreadAsync(long userId, CancellationToken cancellationToken = default);
}
