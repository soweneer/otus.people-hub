using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Repositories;

public interface IDialogRepository
{
    Task AddAsync(DialogMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default);
}
