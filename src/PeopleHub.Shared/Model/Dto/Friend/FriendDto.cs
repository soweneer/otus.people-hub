using PeopleHub.Shared.Model.Dto.Person;
using PeopleHub.Domain.Enums;

namespace PeopleHub.Shared.Model.Dto.Friend;

public sealed record FriendDto(PersonLiteDto Person, int FriendRequestId, FriendRequestStatus FriendRequestStatus);