namespace PeopleHub.Chats.Messaging;

public sealed class RabbitMqOptions
{
    public string ConnectionString { get; set; }
    public string ClientName { get; set; } = "people-hub-chats";
}
