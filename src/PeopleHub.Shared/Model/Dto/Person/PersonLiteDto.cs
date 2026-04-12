namespace PeopleHub.Shared.Model.Dto.Person;

public sealed record PersonLiteDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int Age { get; init; }
    public string City { get; init; }
}
