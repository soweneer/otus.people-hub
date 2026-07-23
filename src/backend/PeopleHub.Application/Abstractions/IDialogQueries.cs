using PeopleHub.Application.Models;

namespace PeopleHub.Application.Abstractions;

public interface IDialogQueries
{
    Task<IReadOnlyCollection<DialogPartner>> GetPartnersAsync(long userId, CancellationToken cancellationToken = default);
}
