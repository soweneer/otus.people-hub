using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Exceptions;

namespace PeopleHub.Domain.Entities;

public sealed class FriendRequest(long id, long senderUserId, long receiverUserId, FriendRequestStatus status)
{
    public long Id { get; } = id;
    public long SenderUserId { get; } = senderUserId;
    public long ReceiverUserId { get; } = receiverUserId;
    public FriendRequestStatus Status { get; private set; } = status;

    public static FriendRequest Send(long senderUserId, long receiverUserId)
    {
        EnsureDifferentUsers(senderUserId, receiverUserId);
        return new FriendRequest(0, senderUserId, receiverUserId, FriendRequestStatus.Sent);
    }

    public static FriendRequest EstablishFriendship(long userId, long friendUserId)
    {
        EnsureDifferentUsers(userId, friendUserId);
        return new FriendRequest(0, userId, friendUserId, FriendRequestStatus.Approved);
    }

    public void Approve(long byUserId)
    {
        EnsureReceiver(byUserId);
        Status = FriendRequestStatus.Approved;
    }

    public void Reject(long byUserId)
    {
        EnsureReceiver(byUserId);
        Status = FriendRequestStatus.Rejected;
    }

    public void MarkApproved() => Status = FriendRequestStatus.Approved;

    private void EnsureReceiver(long userId)
    {
        if (userId != ReceiverUserId)
        {
            throw new DomainException($"Заявка [{Id}] адресована другому пользователю");
        }
    }

    private static void EnsureDifferentUsers(long senderUserId, long receiverUserId)
    {
        if (senderUserId == receiverUserId)
        {
            throw new DomainException("Нельзя добавить в друзья самого себя");
        }
    }
}
