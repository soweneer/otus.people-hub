namespace PeopleHub.Shared.Model.Dto.Friend;

public sealed record FriendsInfoDto
{
    public List<FriendDto> Friends { get; } = [];
    public List<FriendDto> IncomingRequests { get; } = [];
    public List<FriendDto> OutgoingRequests { get; } = [];
}
