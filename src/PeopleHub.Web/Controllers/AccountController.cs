using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Auth;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Services;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    public class AccountController(IAccountService accountService, JwtTokenIssuer tokenIssuer) : Controller
    {
        [HttpPost("/login")]
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

        [HttpGet, AllowAnonymous]
        public IActionResult SignIn()
        {
            return User.Identity!.IsAuthenticated
                ? RedirectToAction("Index", "User")
                : View("SignIn");
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            if (await accountService.CanLoginAsync(request.Email, request.Password, HttpContext.RequestAborted))
            {
                await Authenticate(request.Email);
                return RedirectToAction("Index", "User");
            }

            ModelState.AddModelError("Password", "Неверные данные пользователя");
            return View(request);
        }

        [HttpGet, Authorize]
        public new async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("SignIn", "Account");
        }

        [HttpGet, AllowAnonymous]
        public IActionResult SignUp()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "User");

            return View();
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "User");
            if (!ModelState.IsValid)
                return View(request);

            var status = await accountService.SignUpAsync(request.Email, request.Password, 
                request.ExtractPersonalInfo(),
                HttpContext.RequestAborted);

            switch (status)
            {
                case SignUpStatus.AlreadyExists:
                    ModelState.AddModelError("Email", "Такой пользователь уже существует в базе");
                    return View(request);
                case SignUpStatus.Success:
                    await Authenticate(request.Email);
                    return RedirectToAction("Index", "User");
                case SignUpStatus.Failed:
                default:
                    ModelState.AddModelError("Email", "Не удалось зарегистрировать пользователя");
                    return View(request);
            }
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
}
