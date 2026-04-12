namespace PeopleHub.Shared.Model.Dto.Account;

public sealed record AccountDto
{
    public string Email { get; init; }
    public string Password { get; init; }
}
