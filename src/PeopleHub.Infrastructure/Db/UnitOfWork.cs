using PeopleHub.Application.Abstractions;

namespace PeopleHub.Infrastructure.Db;

internal sealed class UnitOfWork(DbClient dbClient) : IUnitOfWork
{
    public Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default) =>
        dbClient.ExecuteInTransactionAsync(action, cancellationToken);
}
