namespace PeopleHub.Counters.Storage;

public sealed record CounterState(long Count, long Total);

public sealed record PartnerUnreadState(long PartnerId, long LastReadMessageId, IReadOnlyList<long> UnreadMessageIds);

public interface ICounterStore
{
    Task<CounterState> ApplyMessageAsync(long userId, long partnerId, long messageId, CancellationToken cancellationToken = default);

    Task<CounterState> ApplyReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default);

    Task<CounterSnapshot> GetAsync(long userId, CancellationToken cancellationToken = default);

    Task<long> GetTotalAsync(long userId, CancellationToken cancellationToken = default);

    Task<long> ResetAsync(long userId, IReadOnlyCollection<PartnerUnreadState> partners, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<long>> GetKnownUserIdsAsync(CancellationToken cancellationToken = default);
}
