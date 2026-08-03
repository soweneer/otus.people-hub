namespace PeopleHub.Counters.Messaging;

public sealed record MessageSentEvent(long MessageId, long DialogId, long FromUserId, long ToUserId);

public sealed record DialogReadEvent(long DialogId, long UserId, long PartnerId, long LastReadMessageId);
