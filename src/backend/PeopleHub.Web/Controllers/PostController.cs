using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
public sealed class PostController : ControllerBase
{
    private readonly int _userId;

    public PostController(int userId)
    {
        _userId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpPost("/api/post/create")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Create(
        [FromBody] CreatePostRequest request,
        [FromServices] IPostService postService)
    {
        if (string.IsNullOrWhiteSpace(request?.Text))
        {
            return Results.BadRequest("Параметр text обязателен");
        }

        var userEmail = User.Identity!.Name;
        var postId = await postService.CreateAsync(
            userEmail,
            request.Text.Trim(),
            HttpContext.RequestAborted);

        return postId is null
            ? Results.Problem("Не удалось создать пост")
            : Results.Json(postId.Value.ToString());
    }

    [HttpPut("/api/post/update")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Update(
        [FromBody] UpdatePostRequest request,
        [FromServices] IPostService postService)
    {
        if (!long.TryParse(request?.Id, out var postId))
        {
            return Results.BadRequest("Параметр id обязателен и должен быть числом");
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Results.BadRequest("Параметр text обязателен");
        }

        var userEmail = User.Identity!.Name;
        var updated = await postService.UpdateAsync(
            userEmail,
            postId,
            request.Text.Trim(),
            HttpContext.RequestAborted);

        return updated
            ? Results.Ok("Успешно изменен пост")
            : Results.BadRequest($"Пост [{request.Id}] не найден или принадлежит другому пользователю");
    }

    [Authorize]
    [HttpPut("/api/post/delete/{id}")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Delete(string id, [FromServices] IPostService postService)
    {
        if (!long.TryParse(id, out var postId))
        {
            return Results.BadRequest("Параметр id обязателен и должен быть числом");
        }

        var userEmail = User.Identity!.Name;
        var deleted = await postService.DeleteAsync(
            userEmail,
            postId,
            HttpContext.RequestAborted);

        return deleted
            ? Results.Ok("Успешно удален пост")
            : Results.BadRequest($"Пост [{id}] не найден или принадлежит другому пользователю");
    }

    [HttpGet("/api/post/get/{id}")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetById(string id, [FromServices] IPostService postService)
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

    [HttpGet("/api/post/feed")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PostResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Feed([FromServices] IFeedService feedService)
    {
        var posts = await feedService.GetFeedAsync(_userId, HttpContext.RequestAborted);

        return Results.Json(posts
            .Select(p => new PostResponse(p.Id.ToString(), p.Text, p.AuthorUserId.ToString()))
            .ToArray());
    }
}
