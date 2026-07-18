using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers.Api;

[ApiController]
[Route("api/users")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class UsersApiController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserInfo>>> Search(
        [FromQuery] SearchUserRequest filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var users = await userService.SearchWithFriendStatusAsync(
            User.Identity?.Name,
            new SearchFilter(
                filter.FirstName?.Trim() ?? string.Empty,
                filter.LastName?.Trim() ?? string.Empty,
                Math.Max(skip, 0),
                Math.Clamp(take, 1, 100)),
            HttpContext.RequestAborted);

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FriendInfo>> GetById(int id)
    {
        var user = await userService.GetWithFriendStatusAsync(User.Identity?.Name, id, HttpContext.RequestAborted);

        return user is null
            ? NotFound(new ApiError($"Пользователь [{id}] не найден"))
            : Ok(user);
    }
}
