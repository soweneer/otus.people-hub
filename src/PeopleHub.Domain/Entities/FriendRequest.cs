using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.Entities;

public sealed class FriendRequest
{
    private FriendRequest(int id, int senderUserId, int receiverUserId, FriendRequestStatus status)
    {
        Id = id;
        SenderUserId = senderUserId;
        ReceiverUserId = receiverUserId;
        Status = status;
    }

    public int Id { get; }
    public int SenderUserId { get; }
    public int ReceiverUserId { get; }
    public FriendRequestStatus Status { get; private set; }

    public static FriendRequest Send(int senderUserId, int receiverUserId)
    {
        EnsureDifferentUsers(senderUserId, receiverUserId);
        return new FriendRequest(0, senderUserId, receiverUserId, FriendRequestStatus.Sent);
    }

    public static FriendRequest EstablishFriendship(int userId, int friendUserId)
    {
        EnsureDifferentUsers(userId, friendUserId);
        return new FriendRequest(0, userId, friendUserId, FriendRequestStatus.Approved);
    }

    public static FriendRequest Restore(int id, int senderUserId, int receiverUserId, FriendRequestStatus status) =>
        new(id, senderUserId, receiverUserId, status);

    public void Approve(int byUserId)
    {
        EnsureReceiver(byUserId);
        Status = FriendRequestStatus.Approved;
    }

    public void Reject(int byUserId)
    {
        EnsureReceiver(byUserId);
        Status = FriendRequestStatus.Rejected;
    }

    public void MarkApproved() => Status = FriendRequestStatus.Approved;

    private void EnsureReceiver(int userId)
    {
        if (userId != ReceiverUserId)
        {
            throw new DomainException($"Заявка [{Id}] адресована другому пользователю");
        }
    }

    private static void EnsureDifferentUsers(int senderUserId, int receiverUserId)
    {
        if (senderUserId == receiverUserId)
        {
            throw new DomainException("Нельзя добавить в друзья самого себя");
        }
    }
}
