using System.Collections.Concurrent;
using nanoFramework.Tarantool;
using nanoFramework.Tarantool.Client.Interfaces;
using nanoFramework.Tarantool.Model;
using nanoFramework.Tarantool.Model.Responses;

namespace PeopleHub.Chats.Tarantool;

internal sealed class TarantoolConnectionPool(string connectionString, int poolSize, int readBufferSize) : IDisposable
{
    private readonly SemaphoreSlim _slots = new(poolSize, poolSize);
    private readonly ConcurrentQueue<IBox> _idle = new();

    public async Task<TResult> CallAsync<TResult>(
        string function,
        TarantoolTuple arguments,
        Func<DataResponse, TResult> parse,
        CancellationToken cancellationToken = default)
    {
        await _slots.WaitAsync(cancellationToken);

        try
        {
            var box = Rent();

            try
            {
                var result = parse(box.Call(function, arguments, typeof(TarantoolTuple)));
                _idle.Enqueue(box);

                return result;
            }
            catch (TarantoolException)
            {
                _idle.Enqueue(box);
                throw;
            }
            catch
            {
                box.Dispose();
                throw;
            }
        }
        finally
        {
            _slots.Release();
        }
    }

    private IBox Rent() =>
        _idle.TryDequeue(out var box) && box.IsConnected
            ? box
            : TarantoolContext.Connect(CreateOptions());

    private ClientOptions CreateOptions()
    {
        var options = new ClientOptions(connectionString);
        options.ConnectionOptions.ReadSchemaOnConnect = false;
        options.ConnectionOptions.ReadBoxInfoOnConnect = false;
        options.ConnectionOptions.ReadStreamBufferSize = readBufferSize;

        return options;
    }

    public void Dispose()
    {
        while (_idle.TryDequeue(out var box))
        {
            box.Dispose();
        }

        _slots.Dispose();
    }
}
