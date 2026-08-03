namespace PeopleHub.Chats.Outbox;

public sealed record DialogReadPayload(long DialogId, long UserId, long PartnerId, long LastReadMessageId);
