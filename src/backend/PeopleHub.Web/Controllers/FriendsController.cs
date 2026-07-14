using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Services;

namespace PeopleHub.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FriendsController(IFriendRequestService friendRequestService) : Controller
    {
        [HttpPut("/api/friend/set/{user_id}")]
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
                friendUserId,
                HttpContext.RequestAborted);

            return added
                ? Results.Ok("Пользователь успешно добавлен в друзья")
                : Results.BadRequest($"Пользователь [{userId}] не найден или является текущим пользователем");
        }

        [HttpPut("/api/friend/delete/{user_id}")]
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
                friendUserId,
                HttpContext.RequestAborted);

            return Results.Ok("Пользователь успешно удален из друзей");
        }
    }
}
