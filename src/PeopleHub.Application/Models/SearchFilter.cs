namespace PeopleHub.Application.Models;

public sealed record SearchFilter(string FirstName, string LastName, int Skip, int Take);
