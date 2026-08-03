namespace PeopleHub.Model;

public sealed record UnreadCounterResponse(long PartnerId, long Count);

public sealed record UnreadCountersResponse(IReadOnlyCollection<UnreadCounterResponse> Counters, long Total);
