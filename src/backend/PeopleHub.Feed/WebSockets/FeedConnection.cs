using System.Net.WebSockets;

namespace PeopleHub.Feed.WebSockets;

public sealed class FeedConnection(WebSocket socket) : IDisposable
{
    private readonly SemaphoreSlim _sendGate = new(1, 1);

    public Guid Id { get; } = Guid.NewGuid();

    public async Task SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (socket.State != WebSocketState.Open)
        {
            return;
        }

        await _sendGate.WaitAsync(cancellationToken);
        try
        {
            await socket.SendAsync(payload, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
        }
        finally
        {
            _sendGate.Release();
        }
    }

    public async Task WaitUntilClosedAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[512];
        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                return;
            }
        }
    }

    public void Dispose() => _sendGate.Dispose();
}
