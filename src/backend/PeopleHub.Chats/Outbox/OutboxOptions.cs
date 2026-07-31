namespace PeopleHub.Chats.Outbox;

public sealed class OutboxOptions
{
    public int BatchSize { get; set; } = 200;
    public int PollIntervalMs { get; set; } = 500;
}
