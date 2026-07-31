namespace PeopleHub.Chats.Outbox;

public static class CountersTopology
{
    public const string ChangedExchange = "counters.changed";
    public const string ApplyQueue = "counters.apply";
    public const string ApplyBindingKey = "#";
}
