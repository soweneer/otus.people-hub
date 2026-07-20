namespace PeopleHub.Domain.Exceptions;

public sealed class UnknownUserException(long id) : Exception($"Пользователь [{id}] не найден в базе данных");
