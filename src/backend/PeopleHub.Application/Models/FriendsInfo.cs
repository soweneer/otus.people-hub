namespace PeopleHub.Application.Models;

public sealed record FriendInfoLite(UserLite User, long FriendRequestId);

public sealed record FriendsInfo(
    IReadOnlyCollection<FriendInfoLite> Friends,
    IReadOnlyCollection<FriendInfoLite> Incoming,
    IReadOnlyCollection<FriendInfoLite> Outgoing
);
