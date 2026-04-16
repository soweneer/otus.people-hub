using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Repositories;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class FriendsController(
        IFriendRepository friendRepository,
        IPersonRepository personRepository) : Controller
    {
        private const string AccessDeniedMessage = "Ошибка прав доступа";

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null)
                return Unauthorized();

            var personId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);
            var friendsInfo = await friendRepository.GetFriendsAsync(personId);
            return View(friendsInfo);
        }

        [HttpGet]
        public async Task<IActionResult> SendFriendRequest(int targetPersonId, string returnUrl)
        {
            if (User.Identity is null)
                return Unauthorized();

            var senderPersonId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);
            await friendRepository.SendAsync(senderPersonId, targetPersonId);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index", "Person")
                : Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> CancelRequest(int targetPersonId, string returnUrl)
        {
            if (User.Identity is null)
                return Unauthorized();

            var senderPersonId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);
            await friendRepository.DeleteAsync(senderPersonId, targetPersonId);

            return string.IsNullOrWhiteSpace(returnUrl)
                ? RedirectToAction("Index")
                : Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int requestId)
        {
            if (User.Identity is null)
                return Unauthorized();

            var personId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);
            var friendRequestInfo = await friendRepository.GetAsync(requestId);
            if (friendRequestInfo.ReceiverPersonId != personId)
                return BadRequest(AccessDeniedMessage);

            await friendRepository.ApproveAsync(requestId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int requestId)
        {
            if (User.Identity is null)
                return Unauthorized();

            var personId = await personRepository.GetPersonIdAsync(User.Identity.Name,
                HttpContext.RequestAborted);
            var friendRequestInfo = await friendRepository.GetAsync(requestId);
            if (friendRequestInfo.ReceiverPersonId != personId)
                return BadRequest(AccessDeniedMessage);

            await friendRepository.RejectAsync(requestId);
            return RedirectToAction("Index");
        }
    }
}
