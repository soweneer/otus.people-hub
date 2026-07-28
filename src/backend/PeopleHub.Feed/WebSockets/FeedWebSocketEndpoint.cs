using System.Security.Claims;

namespace PeopleHub.Feed.WebSockets;

public static class FeedWebSocketEndpoint
{
    private const string Path = "/post/feed/posted";

    public static WebApplication MapFeedWebSocket(this WebApplication app)
    {
        app.Map(Path, HandleAsync).RequireAuthorization();

        return app;
    }

    private static async Task HandleAsync(
        HttpContext context,
        FeedSubscriber subscriber,
        ILogger<FeedSubscriber> logger)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Ожидается WebSocket-соединение");

            return;
        }

        if (!long.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        using var connection = new FeedConnection(socket);

        await subscriber.SubscribeAsync(userId, connection, context.RequestAborted);
        try
        {
            await connection.WaitUntilClosedAsync(context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Соединение пользователя {UserId} закрылось с ошибкой", userId);
        }
        finally
        {
            await subscriber.UnsubscribeAsync(userId, connection, CancellationToken.None);
        }
    }
}
