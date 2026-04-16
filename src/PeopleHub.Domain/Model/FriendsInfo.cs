using PeopleHub.Domain.Enums;

namespace PeopleHub.Domain.Model;

public sealed record PersonLite(int Id, string Name, int Age, string City);

public sealed record FriendInfo(PersonLite Person, int FriendRequestId, FriendRequestStatus FriendRequestStatus);

public sealed record FriendsInfo(
    FriendInfo[] Friends,
    FriendInfo[] IncomingRequests,
    FriendInfo[] OutgoingRequests
);
