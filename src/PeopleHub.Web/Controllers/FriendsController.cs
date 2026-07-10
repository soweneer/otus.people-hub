using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Services;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class FriendsController(IFriendRequestService friendRequestService, IUserService userService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var friendsInfo = await friendRequestService.GetFriendsAsync(User.Identity!.Name, HttpContext.RequestAborted);

            return View(friendsInfo);
        }

        [HttpGet]
        public async Task<IActionResult> SendFriendRequest(int targetUserId, string returnUrl)
        {
            await friendRequestService.SendAsync(User.Identity!.Name, targetUserId,
                HttpContext.RequestAborted);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index", "User")
                : Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> CancelRequest(int targetUserId, string returnUrl)
        {
            await friendRequestService.CancelAsync(User.Identity!.Name, targetUserId, HttpContext.RequestAborted);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index")
                : Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int friendRequestId)
        {
            await friendRequestService.ApproveAsync(User.Identity!.Name, friendRequestId, HttpContext.RequestAborted);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int friendRequestId)
        {
            await friendRequestService.RejectAsync(User.Identity!.Name, friendRequestId, HttpContext.RequestAborted);

            return RedirectToAction("Index");
        }
    }
}
