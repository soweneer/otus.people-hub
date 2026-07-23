using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Models;
using PeopleHub.Domain.Entities;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Application.Services;

public class DialogService(IDialogRepository dialogRepository, IDialogQueries dialogQueries) : IDialogService
{
    public Task<long?> SendAsync(long fromUserId, long toUserId, string text, CancellationToken cancellationToken = default) =>
        dialogRepository.AddAsync(DialogMessage.Create(fromUserId, toUserId, text), cancellationToken);

    public Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default) =>
        dialogRepository.GetDialogAsync(userId1, userId2, cancellationToken);

    public Task<IReadOnlyCollection<DialogPartner>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default) =>
        dialogQueries.GetPartnersAsync(userId, cancellationToken);
}
