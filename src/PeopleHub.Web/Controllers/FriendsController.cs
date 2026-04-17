using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class FriendsController(IFriendRequestService friendRequestService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var friendsInfo = await friendRequestService.GetFriendsAsync(User.Identity!.Name, HttpContext.RequestAborted);
            
            return View(friendsInfo);
        }

        [HttpGet]
        public async Task<IActionResult> SendFriendRequest(int targetPersonId, string returnUrl)
        {
            await friendRequestService.SendAsync(User.Identity!.Name, targetPersonId,
                HttpContext.RequestAborted);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index", "Person")
                : Redirect(returnUrl);
        }

        [HttpPatch]
        public async Task<IActionResult> CancelRequest(int targetPersonId, string returnUrl)
        {
            await friendRequestService.CancelAsync(User.Identity!.Name, targetPersonId, HttpContext.RequestAborted);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index")
                : Redirect(returnUrl);
        }

        [HttpPatch]
        public async Task<IActionResult> Approve(int friendRequestId)
        {
            await friendRequestService.ApproveAsync(User.Identity!.Name, friendRequestId, HttpContext.RequestAborted);
            
            return RedirectToAction("Index");
        }

        [HttpPatch]
        public async Task<IActionResult> Reject(int friendRequestId)
        {
            await friendRequestService.RejectAsync(User.Identity!.Name, friendRequestId, HttpContext.RequestAborted);
            
            return RedirectToAction("Index");
        }
    }
}
