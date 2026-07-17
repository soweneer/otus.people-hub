using PeopleHub.Domain.ValueObjects;

namespace PeopleHub.Domain.Entities;

public sealed class Account(int id, Email email, PasswordHash password, int userId)
{
    public int Id { get; } = id;
    public Email Email { get; } = email;
    public PasswordHash Password { get; } = password;
    public int UserId { get; } = userId;

    public static Account Create(Email email, PasswordHash password, int userId) =>
        new(0, email, password, userId);
}
