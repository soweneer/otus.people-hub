using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Repositories;

namespace PeopleHub.Chats.Services;

internal sealed class DialogService(IDialogRepository dialogRepository) : IDialogService
{
    public async Task<long> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default)
    {
        var message = DialogMessage.Create(fromUserId, text);
        var sent = await dialogRepository.AddMessageAsync(fromUserId, toUserId, message, cancellationToken);

        return sent.MessageId;
    }

    public async Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default)
    {
        var dialogId = await dialogRepository.GetDialogIdAsync(userId1, userId2, cancellationToken);

        return dialogId is null
            ? []
            : await dialogRepository.GetMessagesAsync(dialogId.Value, cancellationToken);
    }

    public Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default) =>
        dialogRepository.GetPartnerIdsAsync(userId, cancellationToken);

    public Task<long> MarkReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default) =>
        dialogRepository.MarkReadAsync(userId, partnerId, upToMessageId, cancellationToken);

    public Task<IReadOnlyCollection<UnreadCount>> GetUnreadCountsAsync(long userId, CancellationToken cancellationToken = default) =>
        dialogRepository.GetUnreadCountsAsync(userId, cancellationToken);
}
