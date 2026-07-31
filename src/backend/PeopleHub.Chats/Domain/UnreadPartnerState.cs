namespace PeopleHub.Chats.Domain;

public sealed record UnreadPartnerState(long PartnerId, long LastReadMessageId, IReadOnlyList<long> UnreadMessageIds);
