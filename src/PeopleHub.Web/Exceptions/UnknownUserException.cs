namespace PeopleHub.Exceptions;

public sealed class UnknownUserException(int id) : Exception($"Пользователь [{id}] не найден в базе");
