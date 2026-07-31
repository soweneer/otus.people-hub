namespace PeopleHub.Counters.Storage;

public interface ICounterStore
{
    Task<long> ApplyMessageAsync(long userId, long partnerId, long messageId, CancellationToken cancellationToken = default);

    Task<long> ApplyReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default);

    Task<CounterSnapshot> GetAsync(long userId, CancellationToken cancellationToken = default);

    Task<long> GetTotalAsync(long userId, CancellationToken cancellationToken = default);
}
