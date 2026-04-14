namespace PeopleHub.Domain.Model.Dto.Friend;

public sealed record FriendsInfoDto(
    FriendDto[] Friends,
    FriendDto[] IncomingRequests,
    FriendDto[] OutgoingRequests
);
