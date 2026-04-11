namespace PeopleHub.Lib.Model.Dto.Friend;

public sealed record DtoFriendRequest
{
    public int Id { get; init; }
    public int SenderPersonId { get; init; }
    public int ReceiverPersonId { get; init; }
}
