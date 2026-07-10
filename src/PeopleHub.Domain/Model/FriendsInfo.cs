using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record FriendInfoLite(UserLite User, int FriendRequestId);

public sealed record FriendsInfo(
    IReadOnlyCollection<FriendInfoLite> Friends,
    IReadOnlyCollection<FriendInfoLite> Incoming,
    IReadOnlyCollection<FriendInfoLite> Outgoing
);
