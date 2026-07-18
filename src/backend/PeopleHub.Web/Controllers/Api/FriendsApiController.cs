using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Models;
using PeopleHub.Application.Services;
using PeopleHub.Model;

namespace PeopleHub.Controllers.Api;

[ApiController]
[Route("api/friends")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class FriendsApiController(IFriendRequestService friendRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FriendsInfo>> Get()
    {
        return Ok(await friendRequestService.GetFriendsAsync(User.Identity?.Name, HttpContext.RequestAborted));
    }

    [HttpPost("requests")]
    public async Task<IActionResult> Send([FromBody] SendFriendRequestBody body)
    {
        await friendRequestService.SendAsync(User.Identity?.Name, body.TargetUserId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Cancel(int userId)
    {
        await friendRequestService.CancelAsync(User.Identity?.Name, userId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("requests/{requestId:int}/approve")]
    public async Task<IActionResult> Approve(int requestId)
    {
        await friendRequestService.ApproveAsync(User.Identity?.Name, requestId, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPost("requests/{requestId:int}/reject")]
    public async Task<IActionResult> Reject(int requestId)
    {
        await friendRequestService.RejectAsync(User.Identity?.Name, requestId, HttpContext.RequestAborted);

        return NoContent();
    }
}
