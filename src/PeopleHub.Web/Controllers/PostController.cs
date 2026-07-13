using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    [Authorize]
    public class PostController(IPostService postService) : Controller
    {
        [HttpPost("/post/create")]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Create([FromBody] CreatePostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Text))
            {
                return Results.BadRequest("Параметр text обязателен");
            }

            var postId = await postService.CreateAsync(
                User.Identity!.Name,
                request.Text.Trim(),
                HttpContext.RequestAborted);

            return postId is null
                ? Results.Problem("Не удалось создать пост")
                : Results.Json(postId.Value.ToString());
        }
    }
}
