using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Model;
using PeopleHub.Exceptions;
using PeopleHub.Extensions;
using PeopleHub.Domain.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class UserController(IUserService userService) : Controller
    {
        private const int PageSize = 20;

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.PageSize = PageSize;

            return View(Array.Empty<UserInfo>());
        }

        [HttpGet]
        public async Task<IActionResult> LoadMore(int skip, string firstName, string lastName)
        {
            var users = await userService.SearchAsync(
                User.Identity!.Name,
                new SearchFilter(firstName ?? string.Empty, lastName ?? string.Empty, skip, PageSize),
                HttpContext.RequestAborted);

            return PartialView("_UserRows", users);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IResult> Search(SearchUserPaginatedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var error = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Results.BadRequest(error);
            }

            var firstName = request.FirstName?.Trim() ?? string.Empty;
            var lastName = request.LastName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                return Results.BadRequest("Заполните хотя бы одно из полей: имя или фамилия");
            }

            var skip = request?.Skip ?? 0;
            var take = request?.Take ?? 0;
            var users = await userService.SearchAsync(
                new SearchFilter(firstName, lastName, skip, take),
                HttpContext.RequestAborted);

            return Results.Json(users);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Filter(SearchUserRequest request)
        {
            ViewBag.PageSize = PageSize;
            if (!ModelState.IsValid)
            {
                ViewBag.SearchError = string.Join("; ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View("Index", Array.Empty<UserInfo>());
            }

            var firstName = request.FirstName?.Trim() ?? string.Empty;
            var lastName = request.LastName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                ViewBag.SearchError = "Заполните хотя бы одно из полей: имя или фамилия";
                return View("Index", Array.Empty<UserInfo>());
            }

            ViewBag.SearchFirstName = firstName;
            ViewBag.SearchLastName = lastName;

            return View("Index", Array.Empty<UserInfo>());
        }

        [HttpGet]
        [ActionName("User")]
        public async Task<IActionResult> UserById(int userId)
        {
            var userInfo = await userService.GetByEmailAsync(User.Identity!.Name, userId, HttpContext.RequestAborted);

            return userInfo == null
                ? throw new UnknownUserException(userId)
                : View("User", userInfo);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userInfo = await userService.GetProfileAsync(User.Identity!.Name, HttpContext.RequestAborted);

            return View(userInfo);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdateMyProfileRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var updatedUser = await userService.UpdateAsync(User.Identity!.Name, updateRequest.ExtractPersonalInfo(), HttpContext.RequestAborted);
            TempData["SuccessMessage"] = "Профиль успешно сохранен";

            return View(updatedUser);
        }
    }
}
