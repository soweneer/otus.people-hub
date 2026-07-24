using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Dialogs;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
public sealed class DialogController : ControllerBase
{
    private long UserId => User.GetUserId();

    [HttpPost("/dialog/{user_id}/send")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Send(
        [FromRoute(Name = "user_id")] string userId,
        [FromBody] SendMessageRequest request,
        [FromServices] IDialogService dialogService)
    {
        if (!long.TryParse(userId, out var toUserId))
        {
            return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
        }

        if (string.IsNullOrWhiteSpace(request?.Text))
        {
            return Results.BadRequest("Параметр text обязателен");
        }

        var sent = await dialogService.SendMessageAsync(UserId, toUserId, request.Text.Trim(), HttpContext.RequestAborted);

        return sent
            ? Results.Ok("Сообщение отправлено")
            : Results.Problem("Не удалось отправить сообщение");
    }

    [HttpGet("/dialog/{user_id}/list")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Produces("application/json")]
    [ProducesResponseType(typeof(DialogMessageResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> List(
        [FromRoute(Name = "user_id")] string userId,
        [FromServices] IDialogService dialogService)
    {
        if (!long.TryParse(userId, out var otherUserId))
        {
            return Results.BadRequest("Параметр user_id обязателен и должен быть числом");
        }

        var messages = await dialogService.GetDialogAsync(UserId, otherUserId, HttpContext.RequestAborted);

        return Results.Json(messages);
    }

    [HttpGet("/api/dialog/partners")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<IReadOnlyCollection<DialogPartnerResponse>>> Partners(
        [FromServices] IDialogService dialogService) =>
        Ok(await dialogService.GetPartnersAsync(UserId, HttpContext.RequestAborted));
}
