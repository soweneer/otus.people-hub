using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Enums;
using PeopleHub.Domain.Services;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    public class AccountController(IAccountService accountService) : Controller
    {
        [HttpGet, AllowAnonymous]
        public IActionResult SignIn()
        {
            return User.Identity!.IsAuthenticated
                ? RedirectToAction("Index", "Person")
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
                return RedirectToAction("Index", "Person");
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
                return RedirectToAction("Index", "Person");

            return View();
        }

        [HttpPost, AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpRequest request)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Index", "Person");
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
                    return RedirectToAction("Index", "Person");
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
