namespace PeopleHub.Chats.Tarantool;

internal readonly record struct TarantoolEndpoint(string Host, int Port)
{
    private const int DefaultPort = 3301;

    public static TarantoolEndpoint Parse(string value)
    {
        var parts = value.Split(':', 2);

        return parts.Length == 2 && int.TryParse(parts[1], out var port)
            ? new TarantoolEndpoint(parts[0], port)
            : new TarantoolEndpoint(value, DefaultPort);
    }
}
