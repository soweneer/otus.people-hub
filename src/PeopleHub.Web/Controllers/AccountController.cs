using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Model.Dto.Person;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Helpers;
using PeopleHub.Shared.Model.View;

namespace PeopleHub.Controllers
{
    [AllowAnonymous]
    public class AccountController(
        IMapper mapper,
        IAccountRepository accountRepository,
        IPersonRepository personRepository) : Controller
    {
        [HttpGet]
        public IActionResult SignIn()
        {
            return User.Identity.IsAuthenticated
                ? RedirectToAction("Index", "Person")
                : View("SignIn");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = await accountRepository.FindByEmailAsync(model.Email);
            if (account is not null && Encrypt.VerifyHashedPassword(account.Password, model.Password))
            {
                await Authenticate(model.Email);
                return RedirectToAction("Index", "Person");
            }

            ModelState.AddModelError("Password", "Неверные данные пользователя");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public new async Task<IActionResult> SignOut()
        {
            if (User.Identity is null) return Unauthorized();
            if (User.Identity.IsAuthenticated)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("SignIn", "Account");
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

        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity is null) return Unauthorized();

            return User.Identity.IsAuthenticated
                ? RedirectToAction("Index", "Person")
                : View();
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpModel model)
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Person");

            if (!ModelState.IsValid)
                return View(model);
            if (await accountRepository.ExistsAsync(model.Email))
            {
                ModelState.AddModelError("Email", "Такой пользователь уже существует в базе");
                return View(model);
            }

            var personResult = await personRepository.CreateAsync(
                mapper.Map<SignUpModel, PersonDto>(model), HttpContext.RequestAborted);
            if (personResult.HasValue)
            {
                var hashedPassword = Encrypt.HashPassword(model.Password);
                var accountResult = await accountRepository.CreateAsync(
                    model.Email, hashedPassword, personResult.Value);
                if (accountResult.HasValue)
                {
                    await Authenticate(model.Email);
                    return RedirectToAction("Index", "Person");
                }
            }

            ModelState.AddModelError("Email", "Не удалось зарегистрировать пользователя");
            return View(model);
        }
    }
}
