using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class FriendsController(IFriendRequestService friendRequestService, IPostService postService) : Controller
    {
        private const int FeedLimit = 20;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var friendsInfo = await friendRequestService.GetFriendsAsync(User.Identity!.Name, HttpContext.RequestAborted);
            var feed = await postService.GetFeedAsync(User.Identity!.Name, 0, FeedLimit, HttpContext.RequestAborted);

            return View(new FriendsViewModel(friendsInfo, feed));
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
