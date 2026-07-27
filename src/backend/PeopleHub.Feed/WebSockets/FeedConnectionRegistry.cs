using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace PeopleHub.Feed.WebSockets;

public sealed class FeedConnectionRegistry(ILogger<FeedConnectionRegistry> logger)
{
    private readonly ConcurrentDictionary<long, ConcurrentDictionary<Guid, FeedConnection>> _connections = new();

    public IReadOnlyCollection<long> SubscribedUserIds => _connections.Keys.ToArray();

    public bool Add(long userId, FeedConnection connection)
    {
        var userConnections = _connections.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, FeedConnection>());
        userConnections[connection.Id] = connection;

        return userConnections.Count == 1;
    }

    public bool Remove(long userId, FeedConnection connection)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
        {
            return false;
        }

        userConnections.TryRemove(connection.Id, out _);
        if (!userConnections.IsEmpty)
        {
            return false;
        }

        _connections.TryRemove(userId, out _);

        return true;
    }

    public async Task BroadcastAsync(long userId, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
        {
            return;
        }

        foreach (var connection in userConnections.Values)
        {
            try
            {
                await connection.SendAsync(payload, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Не удалось отправить событие в сокет пользователя {UserId}", userId);
            }
        }
    }
}
