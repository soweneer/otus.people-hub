namespace PeopleHub.Domain.Model;

public sealed record SearchFilter(string FirstName, string LastName, int Skip, int Take);
