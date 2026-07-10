using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record UserInfo(UserLite User, FriendRequestStatus Status);
