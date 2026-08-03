using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Repositories;

namespace PeopleHub.Chats.Services;

internal sealed class DialogService(IDialogRepository dialogRepository) : IDialogService
{
    public async Task<long> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default)
    {
        var message = DialogMessage.Create(fromUserId, text);
        var sent = await dialogRepository.AddMessageAsync(fromUserId, toUserId, message, cancellationToken);

        DialogMetrics.MessagesSent.Inc();

        return sent.MessageId;
    }

    public async Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        var dialogId = await dialogRepository.GetDialogIdAsync(userId1, userId2, cancellationToken);
        if (dialogId is null)
        {
            DialogMetrics.DialogPageSize.Observe(0);

            return [];
        }

        var messages = await dialogRepository.GetMessagesAsync(dialogId.Value, cancellationToken);
        DialogMetrics.DialogPageSize.Observe(messages.Count);

        return messages;
    }

    public Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default) =>
        dialogRepository.GetPartnerIdsAsync(userId, cancellationToken);

    public async Task<long> MarkReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default)
    {
        var lastReadMessageId = await dialogRepository.MarkReadAsync(userId, partnerId, upToMessageId, cancellationToken);

        DialogMetrics.DialogsRead.Inc();

        return lastReadMessageId;
    }

    public async Task<IReadOnlyCollection<UnreadCount>> GetUnreadCountsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var counts = await dialogRepository.GetUnreadCountsAsync(userId, cancellationToken);

        DialogMetrics.UnreadReturned.Observe(counts.Sum(count => count.Count));

        return counts;
    }

    public Task<IReadOnlyCollection<UnreadPartnerState>> GetUnreadStateAsync(long userId, int limitPerPartner,
        CancellationToken cancellationToken = default) =>
        dialogRepository.GetUnreadStateAsync(userId, limitPerPartner, cancellationToken);
}
