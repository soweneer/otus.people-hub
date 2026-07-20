using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class FriendsController : ControllerBase
{
    private readonly IFriendRequestService _friendRequestService;
    private const string AnySchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}";
    private readonly long _userId;

    public FriendsController(IFriendRequestService friendRequestService)
    {
        _friendRequestService = friendRequestService;
        _userId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("/api/friends")]
    [Authorize]
    public async Task<ActionResult<FriendsInfo>> Get() =>
        Ok(await _friendRequestService.GetFriendsAsync(_userId, HttpContext.RequestAborted));

    [HttpPost("/api/friends/requests")]
    [Authorize]
    public async Task<IActionResult> Send([FromBody] SendFriendRequestBody body)
    {
        await _friendRequestService.SendAsync(_userId, body.TargetUserId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpDelete("/api/friends/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> Cancel(long userId)
    {
        await _friendRequestService.CancelAsync(_userId, userId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("/api/friends/requests/{requestId:int}/approve")]
    [Authorize]
    public async Task<IActionResult> Approve(long requestId)
    {
        await _friendRequestService.ApproveAsync(_userId, requestId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("/api/friends/requests/{requestId:int}/reject")]
    [Authorize]
    public async Task<IActionResult> Reject(int requestId)
    {
        await _friendRequestService.RejectAsync(_userId, requestId, HttpContext.RequestAborted);

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
        if (!int.TryParse(userId, out var friendUserId))
        {
            return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
        }

        var added = await _friendRequestService.SetFriendAsync(
            _userId,
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
        if (!int.TryParse(userId, out var friendUserId))
        {
            return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
        }

        await _friendRequestService.CancelAsync(
            _userId,
            friendUserId,
            HttpContext.RequestAborted);

        return Results.Ok("Пользователь успешно удален из друзей");
    }
}
