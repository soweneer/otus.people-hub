using PeopleHub.Model;

namespace PeopleHub.Dialogs;

public interface IDialogGateway
{
    Task<bool> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessageResponse>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogPartnerResponse>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default);
}
