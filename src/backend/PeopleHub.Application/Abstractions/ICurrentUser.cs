namespace PeopleHub.Application.Abstractions;

/// <summary>
/// Текущий аутентифицированный пользователь. Реализация живёт на стороне хоста (Web).
/// </summary>
public interface ICurrentUser
{
    string Email { get; }

    Task<int> GetUserIdAsync(CancellationToken cancellationToken = default);
}
