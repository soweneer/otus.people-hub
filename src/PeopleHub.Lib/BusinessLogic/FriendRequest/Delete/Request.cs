using MediatR;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Delete;

public sealed record Request(string InitiatorEmail, int ReceiverPersonId): IRequest;