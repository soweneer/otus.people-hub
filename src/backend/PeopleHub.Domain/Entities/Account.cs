using PeopleHub.Domain.ValueObjects;

namespace PeopleHub.Domain.Entities;

public sealed class Account
{
    private Account(int id, Email email, PasswordHash password, int userId)
    {
        Id = id;
        Email = email;
        Password = password;
        UserId = userId;
    }

    public int Id { get; }
    public Email Email { get; }
    public PasswordHash Password { get; }
    public int UserId { get; }

    public static Account Create(Email email, PasswordHash password, int userId) =>
        new(0, email, password, userId);

    public static Account Restore(int id, Email email, PasswordHash password, int userId) =>
        new(id, email, password, userId);
}
