namespace PeopleHub.Domain.Model;

public record UpdatePersonData(string Name, string Surname, int Age, string City, string Bio, int Gender);