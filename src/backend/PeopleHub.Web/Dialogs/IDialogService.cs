using PeopleHub.Model;

namespace PeopleHub.Dialogs;

public interface IDialogService
{
    Task<bool> SendMessageAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessageResponse>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogPartnerResponse>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default);

    Task<long> MarkReadAsync(long userId, long partnerId, long upToMessageId, CancellationToken cancellationToken = default);
}
