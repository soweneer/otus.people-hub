using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.Model.Dto.Person;

public sealed record DtoPerson
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Surname { get; init; }
    public int Age { get; init; }
    public string City { get; init; }
    public Gender Gender { get; init; }
    public string Bio { get; init; }
    public FriendRequestStatus Status { get; init; }
}
