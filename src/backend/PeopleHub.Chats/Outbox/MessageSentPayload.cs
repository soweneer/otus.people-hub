namespace PeopleHub.Chats.Outbox;

public sealed record MessageSentPayload(long MessageId, long DialogId, long FromUserId, long ToUserId);
