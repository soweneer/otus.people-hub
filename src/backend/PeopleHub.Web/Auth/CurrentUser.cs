using PeopleHub.Application.Abstractions;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Auth;

/// <summary>
/// Текущий пользователь из claims запроса. Резолв email -> userId выполняется один раз на запрос.
/// </summary>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository) : ICurrentUser
{
    private int? _userId;

    public string Email => httpContextAccessor.HttpContext?.User.Identity?.Name;

    public async Task<int> GetUserIdAsync(CancellationToken cancellationToken = default) =>
        _userId ??= await userRepository.GetUserIdAsync(Email, cancellationToken);
}
