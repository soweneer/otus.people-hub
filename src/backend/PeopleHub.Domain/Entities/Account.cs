using PeopleHub.Domain.ValueObjects;

namespace PeopleHub.Domain.Entities;

public sealed class Account(long id, Email email, PasswordHash password, long userId)
{
    public long Id { get; } = id;
    public Email Email { get; } = email;
    public PasswordHash Password { get; } = password;
    public long UserId { get; } = userId;

    public static Account Create(Email email, PasswordHash password, long userId) =>
        new(0, email, password, userId);
}
