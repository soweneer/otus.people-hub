using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;

namespace PeopleHub.Controllers.Api;

[ApiController]
[Route("api/feed")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class FeedApiController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<FeedPost>>> Get(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 20)
    {
        var posts = await postService.GetFeedAsync(
            Math.Max(offset, 0),
            Math.Clamp(limit, 1, 100),
            HttpContext.RequestAborted);

        return Ok(posts);
    }
}
