namespace PeopleHub.Lib.Exceptions
{
    public sealed class UnknownUserException : Exception
    {
        public UnknownUserException(string email): base($"Пользователь [{email}] не найден в базе данных")
        {
            
        }
    }
}