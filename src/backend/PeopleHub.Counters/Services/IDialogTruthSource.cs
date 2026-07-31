using PeopleHub.Counters.Storage;

namespace PeopleHub.Counters.Services;

public interface IDialogTruthSource
{
    Task<IReadOnlyCollection<PartnerCount>> GetCountsAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PartnerUnreadState>> GetStateAsync(long userId, CancellationToken cancellationToken = default);
}
