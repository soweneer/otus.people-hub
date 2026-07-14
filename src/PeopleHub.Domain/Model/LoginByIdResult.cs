namespace PeopleHub.Domain.Model;

public enum LoginByIdStatus
{
    Success = 0,
    UserNotFound = 1,
    InvalidPassword = 2
}

public sealed record LoginByIdResult(LoginByIdStatus Status, string Email = null)
{
    public static readonly LoginByIdResult UserNotFound = new(LoginByIdStatus.UserNotFound);
    public static readonly LoginByIdResult InvalidPassword = new(LoginByIdStatus.InvalidPassword);
    public static LoginByIdResult Success(string email) => new(LoginByIdStatus.Success, email);
}
