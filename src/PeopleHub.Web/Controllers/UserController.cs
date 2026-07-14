using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Model;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    public class UserController(IUserService userService) : Controller
    {
        [HttpGet("/api/user/search")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        public async Task<IResult> GetById(int id)
        {
            var user = await userService.GetAsync(id, HttpContext.RequestAborted);

            return user is null
                ? Results.NotFound($"Пользователь [{id}] не найден")
                : Results.Json(new UserResponse(id.ToString(), user.Name, user.Surname, user.Bio, user.City));
        }
    }
}
