using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Model;
using PeopleHub.Domain.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    public class SearchController(ISearchService searchService) : Controller
    {
        [HttpGet]
        public async Task<IResult> Index(SearchPersonPaginatedRequest request)
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
            var persons = await searchService.SearchAsync(
                new SearchFilter(firstName, lastName, skip, take),
                HttpContext.RequestAborted);

            return Results.Json(persons);
        }
    }
}
