using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record PersonInfo(PersonLite Person, FriendRequestStatus Status);