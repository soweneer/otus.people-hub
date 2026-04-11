using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Lib.BusinessLogic.Person.FindByEmail;
using PeopleHub.Lib.Exceptions;

namespace PeopleHub.Controllers
{
    using FindPersonByEmailRequest = Request;
    using GetFriendsInfoRequest = Lib.BusinessLogic.FriendRequest.GetAll.Request;
    using GetFriendRequest = Lib.BusinessLogic.FriendRequest.Get.Request;
    using InitiateFriendshipRequest = Lib.BusinessLogic.FriendRequest.Send.Request;
    using CancelFriendshipRequest = Lib.BusinessLogic.FriendRequest.Delete.Request;
    using ApproveFriendshipRequest = Lib.BusinessLogic.FriendRequest.Approve.Request;
    using RejectFriendshipRequest = Lib.BusinessLogic.FriendRequest.Reject.Request;
    
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IMediator _mediator;
        private const string AccessDeniedMessage = "Ошибка прав доступа";

        public FriendsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null)
                return Unauthorized();

            var friendsInfo = await _mediator.Send(new GetFriendsInfoRequest(User.Identity.Name),
                HttpContext.RequestAborted); 
            return View(friendsInfo);
        }

        [HttpGet]
        public async Task<IActionResult> SendFriendRequest(int targetPersonId, string returnUrl)
        {
            if (User.Identity is null)
                return Unauthorized();

            await _mediator.Send(new InitiateFriendshipRequest(User.Identity.Name, targetPersonId),
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

            await _mediator.Send(new CancelFriendshipRequest(User.Identity.Name, targetPersonId),
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
            
            var personId = await _mediator.Send(new FindPersonByEmailRequest(User.Identity.Name));
            var friendRequestInfo = await _mediator.Send(new GetFriendRequest(requestId), HttpContext.RequestAborted);
            if (friendRequestInfo.ReceiverPersonId != personId)
                return BadRequest(AccessDeniedMessage);

            await _mediator.Send(new ApproveFriendshipRequest(requestId), HttpContext.RequestAborted);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int requestId)
        {
            if (User.Identity is null)
                return Unauthorized();
            
            var personId = await _mediator.Send(new FindPersonByEmailRequest(User.Identity.Name));
            var friendRequestInfo = await _mediator.Send(new GetFriendRequest(requestId), HttpContext.RequestAborted);
            if (friendRequestInfo.ReceiverPersonId != personId)
                return BadRequest(AccessDeniedMessage);

            await _mediator.Send(new RejectFriendshipRequest(requestId), HttpContext.RequestAborted);
            return RedirectToAction("Index");
        }
    }
}