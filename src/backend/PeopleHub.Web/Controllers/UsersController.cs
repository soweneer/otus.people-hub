using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Model;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    private long UserId => int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/api/users")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<UserInfo>>> Search(
        [FromQuery] SearchUserRequest filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var users = await userService.SearchWithFriendStatusAsync(
            UserId,
            new SearchFilter(
                filter.FirstName?.Trim() ?? string.Empty,
                filter.LastName?.Trim() ?? string.Empty,
                Math.Max(skip, 0),
                Math.Clamp(take, 1, 100)),
            HttpContext.RequestAborted);

        return Ok(users);
    }

    [HttpGet("/api/users/{id:int}")]
    [Authorize]
    public async Task<ActionResult<FriendInfo>> GetById(int id)
    {
        var user = await userService.GetWithFriendStatusAsync(UserId, id, HttpContext.RequestAborted);

        return user is null
            ? NotFound(new ApiError($"Пользователь [{id}] не найден"))
            : Ok(user);
    }

    [HttpGet("/api/user/search")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> SearchAnonymous([FromQuery] SearchUserPaginatedRequest request)
    {
        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return Results.BadRequest("Параметры first_name и last_name обязательны");
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        var users = await userService.SearchAsync(
            new SearchFilter(firstName, lastName, skip, take),
            HttpContext.RequestAborted);

        return Results.Json(users
            .Select(u => new UserResponse(u.Id.ToString(), u.FirstName, u.SecondName, u.Biography, u.City))
            .ToArray());
    }

    [HttpPost("/api/user/register")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> Register([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.FirstName) || string.IsNullOrWhiteSpace(request.SecondName))
        {
            return Results.BadRequest("Параметры first_name и second_name обязательны");
        }

        var userId = await userService.CreateAsync(
            new PersonalInfo(
                request.FirstName.Trim(),
                request.SecondName.Trim(),
                request.Age ?? 18,
                request.City ?? string.Empty,
                request.Biography,
                request.Gender ?? 0),
            HttpContext.RequestAborted);

        return userId is null
            ? Results.Problem("Не удалось создать пользователя")
            : Results.Json(new RegisterUserResponse(userId.Value.ToString()));
    }

    [HttpGet("/api/user/{id:int}")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByIdAnonymous(int id)
    {
        var user = await userService.GetAsync(id, HttpContext.RequestAborted);

        return user is null
            ? Results.NotFound($"Пользователь [{id}] не найден")
            : Results.Json(new UserResponse(id.ToString(), user.Name, user.Surname, user.Bio, user.City));
    }
}
