using MediatR;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Send;

public sealed record Request(string SenderPersonEmail, int ReceiverPersonId): IRequest;