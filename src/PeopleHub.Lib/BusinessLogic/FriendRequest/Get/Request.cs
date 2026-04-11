using MediatR;
using PeopleHub.Lib.Model.Dto.Friend;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.Get;

public sealed record Request(int Id): IRequest<DtoFriendRequest>;