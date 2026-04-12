using System.Security.Claims;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Shared.BusinessLogic.Account;
using PeopleHub.Shared.Model.Dto.Person;
using PeopleHub.Shared.Model.View;
using PeopleHub.Infrastructure.Security;

namespace PeopleHub.Controllers
{
    using CreateAccountRequest = CreateRequest;
    using CreatePersonRequest = Shared.BusinessLogic.Person.CreateRequest;
    using FindAccountByEmailRequest = FindByEmailRequest;
    using AccountExistsRequest = ExistsRequest;

    [AllowAnonymous]
    public class AccountController(IMapper mapper, IMediator mediator) : Controller
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

            var account = await mediator.Send(new FindAccountByEmailRequest(model.Email));
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
            if (await mediator.Send(new AccountExistsRequest(model.Email)))
            {
                ModelState.AddModelError("Email", "Такой пользователь уже существует в базе");
                return View(model);
            }

            var personResult = await mediator.Send(new CreatePersonRequest(
                mapper.Map<SignUpModel, PersonDto>(model)));
            if (personResult.HasValue)
            {
                var hashedPassword = Encrypt.HashPassword(model.Password);
                var accountResult = await mediator.Send(
                    new CreateAccountRequest(personResult.Value, model.Email, hashedPassword));
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
