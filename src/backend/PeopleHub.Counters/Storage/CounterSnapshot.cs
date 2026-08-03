namespace PeopleHub.Counters.Storage;

public sealed record PartnerCount(long PartnerId, long Count);

public sealed record CounterSnapshot(IReadOnlyCollection<PartnerCount> Counters, long Total);
