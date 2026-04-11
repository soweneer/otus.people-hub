namespace PeopleHub.Exceptions;

public sealed class UnknownPersonException : Exception
{
    public UnknownPersonException(int personId): base($"Человек [{personId}] не найден в базе")
    { }
}