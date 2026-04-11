using MediatR;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Approve;

public sealed record Request(int Id): IRequest;