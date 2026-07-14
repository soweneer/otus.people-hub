using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        [HttpPut("/post/update")]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Update([FromBody] UpdatePostRequest request)
        {
            if (!long.TryParse(request?.Id, out var postId))
            {
                return Results.BadRequest("Параметр id обязателен и должен быть числом");
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest("Параметр text обязателен");
            }

            var updated = await postService.UpdateAsync(
                User.Identity!.Name,
                postId,
                request.Text.Trim(),
                HttpContext.RequestAborted);

            return updated
                ? Results.Ok("Успешно изменен пост")
                : Results.BadRequest($"Пост [{request.Id}] не найден или принадлежит другому пользователю");
        }

        [HttpPut("/post/delete/{id}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Delete(string id)
        {
            if (!long.TryParse(id, out var postId))
            {
                return Results.BadRequest("Параметр id обязателен и должен быть числом");
            }

            var deleted = await postService.DeleteAsync(
                User.Identity!.Name,
                postId,
                HttpContext.RequestAborted);

            return deleted
                ? Results.Ok("Успешно удален пост")
                : Results.BadRequest($"Пост [{id}] не найден или принадлежит другому пользователю");
        }

        [HttpGet("/post/get/{id}")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> GetById(string id)
        {
            if (!long.TryParse(id, out var postId))
            {
                return Results.BadRequest("Параметр id обязателен и должен быть числом");
            }

            var post = await postService.GetAsync(postId, HttpContext.RequestAborted);

            return post is null
                ? Results.NotFound($"Пост [{id}] не найден")
                : Results.Json(new PostResponse(post.Id.ToString(), post.Text, post.AuthorUserId.ToString()));
        }

        [HttpGet("/post/feed")]
        [ApiExplorerSettings(IgnoreApi = false)]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PostResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IResult> Feed(int offset = 0, int limit = 10)
        {
            if (offset < 0)
            {
                return Results.BadRequest("Параметр offset должен быть не меньше 0");
            }

            if (limit < 1)
            {
                return Results.BadRequest("Параметр limit должен быть не меньше 1");
            }

            var posts = await postService.GetFeedAsync(
                User.Identity!.Name,
                offset,
                limit,
                HttpContext.RequestAborted);

            return Results.Json(posts
                .Select(p => new PostResponse(p.Id.ToString(), p.Text, p.AuthorUserId.ToString()))
                .ToArray());
        }
    }
}
