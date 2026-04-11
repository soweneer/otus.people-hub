using MediatR;
using PeopleHub.Lib.Model.Dto.Friend;

namespace PeopleHub.Lib.BusinessLogic.FriendRequest.GetAll;

public sealed record Request(string PersonEmail): IRequest<DtoFriendsInfo>;