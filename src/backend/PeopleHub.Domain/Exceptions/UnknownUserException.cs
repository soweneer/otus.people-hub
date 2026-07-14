namespace PeopleHub.Domain.Exceptions
{
    public sealed class UnknownUserException(string email)
        : Exception($"Пользователь [{email}] не найден в базе данных");

}
