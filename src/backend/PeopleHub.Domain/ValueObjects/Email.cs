using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.ValueObjects;

public sealed record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string value) =>
        TryCreate(value, out var email)
            ? email
            : throw new DomainException($"Некорректный email: '{value}'");

    public static bool TryCreate(string value, out Email email)
    {
        email = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex == trimmed.Length - 1)
        {
            return false;
        }

        email = new Email(trimmed);
        return true;
    }

    public override string ToString() => Value;
}
