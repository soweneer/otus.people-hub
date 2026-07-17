using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Auth;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

public class AccountController(IAccountService accountService, JwtTokenIssuer tokenIssuer) : Controller
{
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
}
