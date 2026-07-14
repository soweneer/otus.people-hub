using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Enums;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers.Api;

[ApiController]
[Route("api/auth")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AuthApiController(IAccountService accountService) : ControllerBase
{
    [HttpGet("me"), AllowAnonymous]
    public IActionResult Me()
    {
        var authenticated = User.Identity?.IsAuthenticated ?? false;

        return Ok(new MeResponse(authenticated, authenticated ? User.Identity!.Name : null));
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
