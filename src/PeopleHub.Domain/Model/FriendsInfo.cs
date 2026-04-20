using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record FriendLite(PersonLite Person, int FriendRequestId, FriendRequestStatus FriendRequestStatus);

public sealed record FriendsInfo(
    FriendLite[] Friends,
    FriendLite[] Incoming,
    FriendLite[] Outgoing
);
