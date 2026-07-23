namespace PeopleHub.Chats.Db;

public sealed class CitusOptions
{
    public string CoordinatorHost { get; set; }
    public int CoordinatorPort { get; set; } = 5432;
    public int ShardCount { get; set; } = 16;
    public CitusWorker[] Workers { get; set; } = [];
}

public sealed class CitusWorker
{
    public string Host { get; set; }
    public int Port { get; set; } = 5432;
}
