using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record FriendInfo(PersonLite Person, int FriendRequestId, FriendRequestStatus FriendRequestStatus);

public sealed record FriendsInfo(
    FriendInfo[] Friends,
    FriendInfo[] Incoming,
    FriendInfo[] Outgoing
);
