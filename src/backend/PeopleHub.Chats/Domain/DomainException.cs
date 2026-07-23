namespace PeopleHub.Chats.Domain;

public sealed class DomainException(string message) : Exception(message);
