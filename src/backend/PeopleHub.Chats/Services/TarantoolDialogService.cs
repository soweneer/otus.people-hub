using System.Collections;
using nanoFramework.Tarantool.Model;
using nanoFramework.Tarantool.Model.Responses;
using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Tarantool;

namespace PeopleHub.Chats.Services;

internal sealed class TarantoolDialogService(TarantoolConnectionPool connectionPool) : IDialogService
{
    public Task<long> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default) =>
        connectionPool.CallAsync(
            "dialog_send",
            TarantoolTuple.Create(fromUserId, toUserId, text),
            ParseMessageId,
            cancellationToken);

    public Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default) =>
        connectionPool.CallAsync(
            "dialog_list",
            TarantoolTuple.Create(userId1, userId2),
            ParseMessages,
            cancellationToken);

    public Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default) =>
        connectionPool.CallAsync(
            "dialog_partners",
            TarantoolTuple.Create(userId),
            ParsePartnerIds,
            cancellationToken);

    private static long ParseMessageId(DataResponse response) =>
        Convert.ToInt64(SingleReturnValue(response, "dialog_send"));

    private static IReadOnlyCollection<DialogMessage> ParseMessages(DataResponse response)
    {
        if (SingleReturnValue(response, "dialog_list") is not IList rows)
        {
            return [];
        }

        var messages = new DialogMessage[rows.Count];

        for (var i = 0; i < rows.Count; i++)
        {
            if (rows[i] is not IList fields || fields.Count < 4)
            {
                throw new TarantoolException("dialog_list вернул сообщение в неожиданном формате");
            }

            messages[i] = DialogMessage.Restore(
                Convert.ToInt64(fields[0]),
                Convert.ToInt64(fields[1]),
                Convert.ToInt64(fields[2]),
                Convert.ToString(fields[3]));
        }

        return messages;
    }

    private static IReadOnlyCollection<long> ParsePartnerIds(DataResponse response)
    {
        if (SingleReturnValue(response, "dialog_partners") is not IList rows)
        {
            return [];
        }

        var partnerIds = new long[rows.Count];

        for (var i = 0; i < rows.Count; i++)
        {
            partnerIds[i] = Convert.ToInt64(rows[i]);
        }

        return partnerIds;
    }

    private static object SingleReturnValue(DataResponse response, string function)
    {
        if (response?.Data is not { Length: > 0 } data || data[0] is not TarantoolTuple returns || returns.Length == 0)
        {
            throw new TarantoolException($"{function} не вернул результат");
        }

        return returns[0];
    }
}
