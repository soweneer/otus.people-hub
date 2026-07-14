namespace PeopleHub.Application.Abstractions;

/// <summary>
/// Выполняет действие в одной транзакции: коммит при нормальном завершении, откат при исключении.
/// </summary>
public interface IUnitOfWork
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}
