using PeopleHub.Chats.Domain;

namespace PeopleHub.Chats.Services;

public interface IDialogService
{
    Task<long?> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default);
}
