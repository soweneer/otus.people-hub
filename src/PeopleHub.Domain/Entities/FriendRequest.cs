namespace PeopleHub.Domain.Entities;

public sealed record FriendRequest(int Id, int SenderUserId, int ReceiverUserId);
