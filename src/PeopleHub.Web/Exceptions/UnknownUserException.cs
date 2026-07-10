namespace PeopleHub.Exceptions;

public sealed class UnknownUserException(int userId)
    : Exception($"Пользователь [{userId}] не найден в базе");
