using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;

namespace PeopleHub.Application.Services;

public interface IDialogService
{
    Task<long?> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogPartner>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default);
}
