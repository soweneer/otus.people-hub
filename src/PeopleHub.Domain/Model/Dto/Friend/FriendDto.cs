using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model.Dto.Friend;

public sealed record FriendDto(PersonLiteDto Person, int FriendRequestId, FriendRequestStatus FriendRequestStatus);
