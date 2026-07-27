namespace PeopleHub.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string ConnectionString { get; set; }
    public string ClientName { get; set; } = "people-hub";
    public ushort PrefetchCount { get; set; } = 100;
}
