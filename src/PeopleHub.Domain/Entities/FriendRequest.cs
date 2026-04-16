namespace PeopleHub.Domain.Entities;

public sealed record FriendRequest(int Id, int SenderPersonId, int ReceiverPersonId);