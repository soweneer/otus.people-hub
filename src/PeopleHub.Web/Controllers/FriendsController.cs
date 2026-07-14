using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        [HttpPut("/friend/set/{user_id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> SetFriend([FromRoute(Name = "user_id")] string userId)
        {
            if (!int.TryParse(userId, out var friendUserId))
            {
                return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
            }

            var added = await friendRequestService.SetFriendAsync(
                User.Identity!.Name,
                friendUserId,
                HttpContext.RequestAborted);

            return added
                ? Results.Ok("Пользователь успешно добавлен в друзья")
                : Results.BadRequest($"Пользователь [{userId}] не найден или является текущим пользователем");
        }

        [HttpPut("/friend/delete/{user_id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> DeleteFriend([FromRoute(Name = "user_id")] string userId)
        {
            if (!int.TryParse(userId, out var friendUserId))
            {
                return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
            }

            await friendRequestService.CancelAsync(
                User.Identity!.Name,
                friendUserId,
                HttpContext.RequestAborted);

            return Results.Ok("Пользователь успешно удален из друзей");
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
