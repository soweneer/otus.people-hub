using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Repositories;

public interface IDialogRepository
{
    Task<long?> GetDialogIdAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<SentMessage> AddMessageAsync(long fromUserId, long toUserId, DialogMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessage>> GetMessagesAsync(long dialogId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default);

    Task<long> MarkReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UnreadCount>> GetUnreadCountsAsync(long userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UnreadPartnerState>> GetUnreadStateAsync(long userId, int limitPerPartner, CancellationToken cancellationToken = default);
}
