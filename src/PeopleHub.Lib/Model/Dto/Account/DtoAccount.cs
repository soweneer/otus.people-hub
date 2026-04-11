namespace PeopleHub.Lib.Model.Dto.Account;

public sealed record DtoAccount
{
    public string Email { get; init; }
    public string Password { get; init; }
}
