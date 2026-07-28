using System.Buffers;
using MessagePack;
using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Tarantool;

namespace PeopleHub.Chats.Services;

internal sealed class TarantoolDialogService(TarantoolClient client) : IDialogService
{
    public Task<long> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default) =>
        client.CallAsync("dialog_send", [fromUserId, toUserId, text], ParseMessageId, cancellationToken);

    public Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default) =>
        client.CallAsync("dialog_list", [userId1, userId2], ParseMessages, cancellationToken);

    public Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default) =>
        client.CallAsync("dialog_partners", [userId], ParsePartnerIds, cancellationToken);

    private static long ParseMessageId(ReadOnlyMemory<byte> data)
    {
        var reader = new MessagePackReader(new ReadOnlySequence<byte>(data));

        return reader.ReadArrayHeader() == 0
            ? throw new TarantoolException("dialog_send не вернул идентификатор сообщения")
            : reader.ReadInt64();
    }

    private static IReadOnlyCollection<DialogMessage> ParseMessages(ReadOnlyMemory<byte> data)
    {
        var reader = new MessagePackReader(new ReadOnlySequence<byte>(data));

        if (reader.ReadArrayHeader() == 0)
        {
            return [];
        }

        var count = reader.ReadArrayHeader();
        var messages = new DialogMessage[count];

        for (var i = 0; i < count; i++)
        {
            reader.ReadArrayHeader();
            messages[i] = DialogMessage.Restore(
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadString());
        }

        return messages;
    }

    private static IReadOnlyCollection<long> ParsePartnerIds(ReadOnlyMemory<byte> data)
    {
        var reader = new MessagePackReader(new ReadOnlySequence<byte>(data));

        if (reader.ReadArrayHeader() == 0)
        {
            return [];
        }

        var count = reader.ReadArrayHeader();
        var partnerIds = new long[count];

        for (var i = 0; i < count; i++)
        {
            partnerIds[i] = reader.ReadInt64();
        }

        return partnerIds;
    }
}
