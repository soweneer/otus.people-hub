using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.BusinessLogic.Person;

namespace PeopleHub.Controllers
{
    using FindPersonByEmailRequest = FindByEmailRequest;
    using GetFriendsInfoRequest = Domain.BusinessLogic.FriendRequest.GetAllRequest;
    using GetFriendRequest = Domain.BusinessLogic.FriendRequest.GetRequest;
    using InitiateFriendshipRequest = Domain.BusinessLogic.FriendRequest.SendRequest;
    using CancelFriendshipRequest = Domain.BusinessLogic.FriendRequest.DeleteRequest;
    using ApproveFriendshipRequest = Domain.BusinessLogic.FriendRequest.ApproveRequest;
    using RejectFriendshipRequest = Domain.BusinessLogic.FriendRequest.RejectRequest;

    [Authorize]
    public class FriendsController(IMediator mediator) : Controller
    {
        private const string AccessDeniedMessage = "Ошибка прав доступа";

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null)
                return Unauthorized();

            var friendsInfo = await mediator.Send(new GetFriendsInfoRequest(User.Identity.Name),
                HttpContext.RequestAborted);
            return View(friendsInfo);
        }

        [HttpGet]
        public async Task<IActionResult> SendFriendRequest(int targetPersonId, string returnUrl)
        {
            if (User.Identity is null)
                return Unauthorized();

            await mediator.Send(new InitiateFriendshipRequest(User.Identity.Name, targetPersonId),
                HttpContext.RequestAborted);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index", "Person")
                : Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> CancelRequest(int targetPersonId, string returnUrl)
        {
            if (User.Identity is null)
                return Unauthorized();

            await mediator.Send(new CancelFriendshipRequest(User.Identity.Name, targetPersonId),
                HttpContext.RequestAborted);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index")
                : Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int requestId)
        {
            if (User.Identity is null)
                return Unauthorized();

            var personId = await mediator.Send(new FindPersonByEmailRequest(User.Identity.Name));
            var friendRequestInfo = await mediator.Send(new GetFriendRequest(requestId), HttpContext.RequestAborted);
            if (friendRequestInfo.ReceiverPersonId != personId)
                return BadRequest(AccessDeniedMessage);

            await mediator.Send(new ApproveFriendshipRequest(requestId), HttpContext.RequestAborted);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int requestId)
        {
            if (User.Identity is null)
                return Unauthorized();

            var personId = await mediator.Send(new FindPersonByEmailRequest(User.Identity.Name));
            var friendRequestInfo = await mediator.Send(new GetFriendRequest(requestId), HttpContext.RequestAborted);
            if (friendRequestInfo.ReceiverPersonId != personId)
                return BadRequest(AccessDeniedMessage);

            await mediator.Send(new RejectFriendshipRequest(requestId), HttpContext.RequestAborted);
            return RedirectToAction("Index");
        }
    }
}
