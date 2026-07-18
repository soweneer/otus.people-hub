using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Auth;
using PeopleHub.Domain.Enums;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[Route("api/auth")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AuthController(IAccountService accountService, JwtTokenIssuer tokenIssuer) : ControllerBase
{
    [HttpGet("me"), AllowAnonymous]
    public IActionResult Me()
    {
        return Ok(
            HttpContext.User.Identity is not { IsAuthenticated: true } identity
                ? new MeResponse(false) 
                : new MeResponse(true, identity.Name)
            );
    }

    [HttpPost("signin"), AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        if (!await accountService.CanLoginAsync(request.Email, request.Password, HttpContext.RequestAborted))
            return BadRequest(new ApiError("Неверные данные пользователя"));

        await Authenticate(request.Email);

        return Ok(new MeResponse(true, request.Email));
    }

    [HttpPost("signup"), AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        var status = await accountService.SignUpAsync(request.Email, request.Password,
            request.ExtractPersonalInfo(),
            HttpContext.RequestAborted);

        switch (status)
        {
            case SignUpStatus.Success:
                await Authenticate(request.Email);
                return Ok(new MeResponse(true, request.Email));
            case SignUpStatus.AlreadyExists:
                return Conflict(new ApiError("Такой пользователь уже существует в базе"));
            case SignUpStatus.Failed:
            default:
                return BadRequest(new ApiError("Не удалось зарегистрировать пользователя"));
        }
    }

    [HttpPost("signout"), Authorize]
    public async Task<IActionResult> SignOutUser()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    [HttpPost("/api/login")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Login([FromBody] LoginRequest request)
    {
        if (!int.TryParse(request?.Id, out var userId))
        {
            return Results.BadRequest("Параметр id обязателен и должен быть числом");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest("Параметр password обязателен");
        }

        var result = await accountService.LoginByUserIdAsync(userId, request.Password, HttpContext.RequestAborted);

        return result.Status switch
        {
            LoginByIdStatus.Success => Results.Json(new LoginResponse(tokenIssuer.CreateToken(result.Email))),
            LoginByIdStatus.UserNotFound => Results.NotFound($"Пользователь [{request.Id}] не найден"),
            _ => Results.BadRequest("Неверные данные пользователя")
        };
    }

    private async Task Authenticate(string userName)
    {
        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, userName)
        };

        var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
    }
}
