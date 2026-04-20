namespace PeopleHub.Domain.Entities;

public sealed record Account(int Id, string Email, string Password, int PersonId);
