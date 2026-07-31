namespace PeopleHub.Counters.Storage;

public sealed record CounterState(long Count, long Total);

public interface ICounterStore
{
    Task<CounterState> ApplyMessageAsync(long userId, long partnerId, long messageId, CancellationToken cancellationToken = default);

    Task<CounterState> ApplyReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default);

    Task<CounterSnapshot> GetAsync(long userId, CancellationToken cancellationToken = default);

    Task<long> GetTotalAsync(long userId, CancellationToken cancellationToken = default);
}
