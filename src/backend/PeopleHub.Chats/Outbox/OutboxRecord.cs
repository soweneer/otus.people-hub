namespace PeopleHub.Chats.Outbox;

public sealed record OutboxRecord(long Id, string Type, string Payload);
