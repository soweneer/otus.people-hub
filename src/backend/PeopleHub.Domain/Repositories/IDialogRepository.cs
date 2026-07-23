using PeopleHub.Domain.Entities;

namespace PeopleHub.Domain.Repositories;

public interface IDialogRepository
{
    Task<long?> AddAsync(DialogMessage message, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DialogMessage>> GetDialogAsync(long userId1, long userId2, CancellationToken cancellationToken = default);
}
