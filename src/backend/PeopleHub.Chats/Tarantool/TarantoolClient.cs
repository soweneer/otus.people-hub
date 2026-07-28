using System.Collections.Concurrent;

namespace PeopleHub.Chats.Tarantool;

internal sealed class TarantoolClient : IDisposable
{
    private readonly TarantoolEndpoint _endpoint;
    private readonly SemaphoreSlim _slots;
    private readonly ConcurrentQueue<TarantoolConnection> _idle = new();

    public TarantoolClient(TarantoolEndpoint endpoint, int poolSize)
    {
        _endpoint = endpoint;
        _slots = new SemaphoreSlim(poolSize, poolSize);
    }

    public async Task<TResult> CallAsync<TResult>(
        string function,
        object[] arguments,
        Func<ReadOnlyMemory<byte>, TResult> parse,
        CancellationToken cancellationToken = default)
    {
        await _slots.WaitAsync(cancellationToken);

        try
        {
            var connection = await RentAsync(cancellationToken);

            try
            {
                var result = await connection.CallAsync(function, arguments, parse, cancellationToken);
                _idle.Enqueue(connection);

                return result;
            }
            catch (TarantoolException)
            {
                _idle.Enqueue(connection);
                throw;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
        finally
        {
            _slots.Release();
        }
    }

    private Task<TarantoolConnection> RentAsync(CancellationToken cancellationToken) =>
        _idle.TryDequeue(out var connection)
            ? Task.FromResult(connection)
            : TarantoolConnection.ConnectAsync(_endpoint, cancellationToken);

    public void Dispose()
    {
        while (_idle.TryDequeue(out var connection))
        {
            connection.Dispose();
        }

        _slots.Dispose();
    }
}
