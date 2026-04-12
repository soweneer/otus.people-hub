namespace PeopleHub.Exceptions;

public sealed class UnknownPersonException(int personId)
    : Exception($"Человек [{personId}] не найден в базе");