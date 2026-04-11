using MediatR;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Reject;

public sealed record Request(int Id): IRequest;