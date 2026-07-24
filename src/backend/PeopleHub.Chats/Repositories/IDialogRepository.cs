using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Repositories;

public interface IDialogRepository
{
    Task<long> GetOrCreateDialogIdAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<long?> GetDialogIdAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<long> AddMessageAsync(DialogMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessage>> GetMessagesAsync(long dialogId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default);
}
