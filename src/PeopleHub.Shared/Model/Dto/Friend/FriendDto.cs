using PeopleHub.Shared.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Shared.Model.Dto.Friend;

public sealed record FriendDto
{
    public PersonLiteDto Person { get; init; }
    public int FriendRequestId { get; init; }
    public FriendRequestStatus FriendRequestStatus { get; init; }
}
