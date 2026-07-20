using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class FriendsController(IFriendRequestService friendRequestService) : ControllerBase
{
    private const string AnySchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}";
    private long UserId => User.GetUserId();

    [HttpGet("/api/friends")]
    [Authorize]
    public async Task<ActionResult<FriendsInfo>> Get() =>
        Ok(await friendRequestService.GetFriendsAsync(UserId, HttpContext.RequestAborted));

    [HttpPost("/api/friends/requests")]
    [Authorize]
    public async Task<IActionResult> Send([FromBody] SendFriendRequestBody body)
    {
        await friendRequestService.SendAsync(UserId, body.TargetUserId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpDelete("/api/friends/{userId:long}")]
    [Authorize]
    public async Task<IActionResult> Cancel(long userId)
    {
        await friendRequestService.CancelAsync(UserId, userId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("/api/friends/requests/{requestId:long}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(long requestId)
    {
        await friendRequestService.ApproveAsync(UserId, requestId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("/api/friends/requests/{requestId:long}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(long requestId)
    {
        await friendRequestService.RejectAsync(UserId, requestId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPut("/api/friend/set/{user_id}")]
    [Authorize(AuthenticationSchemes = AnySchemes)]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> SetFriend([FromRoute(Name = "user_id")] string userId)
    {
        if (!long.TryParse(userId, out var friendUserId))
        {
            return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
        }

        var added = await friendRequestService.SetFriendAsync(
            UserId,
            friendUserId,
            HttpContext.RequestAborted);

        return added
            ? Results.Ok("Пользователь успешно добавлен в друзья")
            : Results.BadRequest($"Пользователь [{userId}] не найден или является текущим пользователем");
    }

    [HttpPut("/api/friend/delete/{user_id}")]
    [Authorize(AuthenticationSchemes = AnySchemes)]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> DeleteFriend([FromRoute(Name = "user_id")] string userId)
    {
        if (!long.TryParse(userId, out var friendUserId))
        {
            return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
        }

        await friendRequestService.CancelAsync(
            UserId,
            friendUserId,
            HttpContext.RequestAborted);

        return Results.Ok("Пользователь успешно удален из друзей");
    }
}
