namespace PeopleHub.Counters.Messaging;

public static class CountersTopology
{
    public const string ChangedExchange = "counters.changed";
    public const string DeadExchange = "counters.dead";
    public const string PushedExchange = "counters.pushed";
    public const string ApplyQueue = "counters.apply";
    public const string DeadQueue = "counters.dead.messages";
    public const string ApplyBindingKey = "#";

    public const string MessageSentKey = "message.sent";
    public const string DialogReadKey = "dialog.read";

    public static string UserRoutingKey(long userId) => $"user.{userId}";
}
