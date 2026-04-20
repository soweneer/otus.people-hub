using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record FriendInfoLite(PersonLite Person, int FriendRequestId);

public sealed record FriendsInfo(
    IReadOnlyCollection<FriendInfoLite> Friends,
    IReadOnlyCollection<FriendInfoLite> Incoming,
    IReadOnlyCollection<FriendInfoLite> Outgoing
);
