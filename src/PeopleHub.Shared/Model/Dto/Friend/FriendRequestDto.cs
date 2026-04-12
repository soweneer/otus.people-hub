namespace PeopleHub.Shared.Model.Dto.Friend;

public sealed record FriendRequestDto
{
    public int Id { get; init; }
    public int SenderPersonId { get; init; }
    public int ReceiverPersonId { get; init; }
}
