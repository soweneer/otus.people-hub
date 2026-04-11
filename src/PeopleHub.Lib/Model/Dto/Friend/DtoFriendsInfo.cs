namespace PeopleHub.Lib.Model.Dto.Friend;

public sealed record DtoFriendsInfo
{
    public List<DtoFriend> Friends { get; } = [];
    public List<DtoFriend> IncomingRequests { get; } = [];
    public List<DtoFriend> OutgoingRequests { get; } = [];
}
