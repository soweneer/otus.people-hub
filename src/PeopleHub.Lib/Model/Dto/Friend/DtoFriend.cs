using PeopleHub.Lib.Model.Dto.Person;
using PeopleHub.Lib.Model.Enums;

namespace PeopleHub.Lib.Model.Dto.Friend;

public sealed record DtoFriend
{
    public DtoPersonLite Person { get; init; }
    public int FriendRequestId { get; init; }
    public FriendRequestStatus FriendRequestStatus { get; init; }
}
