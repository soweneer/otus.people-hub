namespace PeopleHub.Counters.Messaging;

public sealed class RabbitMqOptions
{
    public string ConnectionString { get; set; }
    public string ClientName { get; set; } = "people-hub-counters";
    public ushort PrefetchCount { get; set; } = 200;
}
