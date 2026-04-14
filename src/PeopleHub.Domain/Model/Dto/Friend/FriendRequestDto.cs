namespace PeopleHub.Domain.Model.Dto.Friend;

public sealed record FriendRequestDto(int Id, int SenderPersonId, int ReceiverPersonId);
