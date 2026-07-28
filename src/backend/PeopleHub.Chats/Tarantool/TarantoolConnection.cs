using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using MessagePack;

namespace PeopleHub.Chats.Tarantool;

internal sealed class TarantoolConnection : IDisposable
{
    private const int GreetingLength = 128;
    private const int LengthPrefixLength = 5;
    private const byte UInt32Marker = 0xce;

    private const byte KeyRequestType = 0x00;
    private const byte KeySync = 0x01;
    private const byte KeyTuple = 0x21;
    private const byte KeyFunctionName = 0x22;
    private const byte KeyData = 0x30;
    private const byte KeyErrorMessage = 0x31;
    private const byte RequestTypeCall = 0x0a;

    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly ArrayBufferWriter<byte> _payload = new(512);
    private readonly byte[] _lengthPrefix = new byte[LengthPrefixLength];

    private byte[] _frame = new byte[1024];
    private byte[] _response = new byte[4096];
    private ulong _sync;

    private TarantoolConnection(TcpClient tcpClient, NetworkStream stream)
    {
        _tcpClient = tcpClient;
        _stream = stream;
    }

    public static async Task<TarantoolConnection> ConnectAsync(TarantoolEndpoint endpoint, CancellationToken cancellationToken)
    {
        var tcpClient = new TcpClient { NoDelay = true };

        try
        {
            await tcpClient.ConnectAsync(endpoint.Host, endpoint.Port, cancellationToken);

            var stream = tcpClient.GetStream();
            var greeting = new byte[GreetingLength];
            await stream.ReadExactlyAsync(greeting, cancellationToken);

            return new TarantoolConnection(tcpClient, stream);
        }
        catch
        {
            tcpClient.Dispose();
            throw;
        }
    }

    public async Task<TResult> CallAsync<TResult>(
        string function,
        object[] arguments,
        Func<ReadOnlyMemory<byte>, TResult> parse,
        CancellationToken cancellationToken)
    {
        await WriteCallAsync(function, arguments, cancellationToken);

        var data = await ReadResponseAsync(cancellationToken);

        return parse(data);
    }

    private async Task WriteCallAsync(string function, object[] arguments, CancellationToken cancellationToken)
    {
        _payload.Clear();

        var writer = new MessagePackWriter(_payload);

        writer.WriteMapHeader(2);
        writer.Write(KeyRequestType);
        writer.Write(RequestTypeCall);
        writer.Write(KeySync);
        writer.Write(++_sync);

        writer.WriteMapHeader(2);
        writer.Write(KeyFunctionName);
        writer.Write(function);
        writer.Write(KeyTuple);
        writer.WriteArrayHeader(arguments.Length);

        foreach (var argument in arguments)
        {
            WriteArgument(ref writer, argument);
        }

        writer.Flush();

        var payload = _payload.WrittenSpan;
        EnsureCapacity(ref _frame, payload.Length + LengthPrefixLength);

        _frame[0] = UInt32Marker;
        BinaryPrimitives.WriteUInt32BigEndian(_frame.AsSpan(1), (uint)payload.Length);
        payload.CopyTo(_frame.AsSpan(LengthPrefixLength));

        await _stream.WriteAsync(_frame.AsMemory(0, payload.Length + LengthPrefixLength), cancellationToken);
    }

    private async Task<ReadOnlyMemory<byte>> ReadResponseAsync(CancellationToken cancellationToken)
    {
        await _stream.ReadExactlyAsync(_lengthPrefix, cancellationToken);

        if (_lengthPrefix[0] != UInt32Marker)
        {
            throw new TarantoolException("Неожиданный формат длины пакета в ответе Tarantool");
        }

        var length = (int)BinaryPrimitives.ReadUInt32BigEndian(_lengthPrefix.AsSpan(1));
        EnsureCapacity(ref _response, length);

        await _stream.ReadExactlyAsync(_response.AsMemory(0, length), cancellationToken);

        return ExtractData(_response, length);
    }

    private static ReadOnlyMemory<byte> ExtractData(byte[] response, int length)
    {
        var reader = new MessagePackReader(new ReadOnlySequence<byte>(response, 0, length));

        long code = -1;
        var headerCount = reader.ReadMapHeader();
        for (var i = 0; i < headerCount; i++)
        {
            if (reader.ReadInt32() == KeyRequestType)
            {
                code = reader.ReadInt64();
                continue;
            }

            reader.Skip();
        }

        ReadOnlyMemory<byte> data = default;
        string error = null;
        var bodyCount = reader.ReadMapHeader();
        for (var i = 0; i < bodyCount; i++)
        {
            switch (reader.ReadInt32())
            {
                case KeyData:
                    var start = (int)reader.Consumed;
                    reader.Skip();
                    data = response.AsMemory(start, (int)reader.Consumed - start);
                    break;
                case KeyErrorMessage:
                    error = reader.ReadString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (code != 0)
        {
            throw new TarantoolException(error ?? $"Tarantool вернул код ошибки {code}");
        }

        return data;
    }

    private static void WriteArgument(ref MessagePackWriter writer, object argument)
    {
        switch (argument)
        {
            case long value:
                writer.Write(value);
                break;
            case string value:
                writer.Write(value);
                break;
            default:
                throw new TarantoolException($"Тип аргумента {argument?.GetType().Name ?? "null"} не поддерживается");
        }
    }

    private static void EnsureCapacity(ref byte[] buffer, int required)
    {
        if (buffer.Length >= required)
        {
            return;
        }

        buffer = new byte[Math.Max(required, buffer.Length * 2)];
    }

    public void Dispose()
    {
        _stream.Dispose();
        _tcpClient.Dispose();
    }
}
