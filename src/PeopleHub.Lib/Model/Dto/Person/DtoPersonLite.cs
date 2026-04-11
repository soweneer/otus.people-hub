namespace PeopleHub.Lib.Model.Dto.Person;

public sealed record DtoPersonLite
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int Age { get; init; }
    public string City { get; init; }
}
