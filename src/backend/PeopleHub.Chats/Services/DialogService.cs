using PeopleHub.Chats.Domain;
using PeopleHub.Chats.Repositories;

namespace PeopleHub.Chats.Services;

internal sealed class DialogService(IDialogRepository dialogRepository) : IDialogService
{
    public Task<long?> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default) =>
        dialogRepository.AddAsync(DialogMessage.Create(fromUserId, toUserId, text), cancellationToken);

    public Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default) =>
        dialogRepository.GetDialogAsync(userId1, userId2, cancellationToken);

    public Task<IReadOnlyCollection<long>> GetPartnerIdsAsync(long userId, CancellationToken cancellationToken = default) =>
        dialogRepository.GetPartnerIdsAsync(userId, cancellationToken);
}
