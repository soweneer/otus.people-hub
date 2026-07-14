using PeopleHub.Domain.Enums;

namespace PeopleHub.Application.Models;

public sealed record UserInfo(UserLite User, FriendRequestStatus Status);
