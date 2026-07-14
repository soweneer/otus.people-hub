using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.ValueObjects;

public sealed record PasswordHash
{
    public PasswordHash(string value) =>
        Value = string.IsNullOrWhiteSpace(value)
            ? throw new DomainException("Хэш пароля не может быть пустым")
            : value;

    public string Value { get; }
}
