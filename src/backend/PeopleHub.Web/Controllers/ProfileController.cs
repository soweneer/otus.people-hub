using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Model;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers;

[Route("api/profile")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ProfileController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PersonalInfo>> Get()
    {
        var userEmail = User.Identity!.Name;
        return Ok(await userService.GetProfileAsync(userEmail, HttpContext.RequestAborted));
    }

    [HttpPut]
    public async Task<ActionResult<PersonalInfo>> Update([FromBody] UpdateMyProfileRequest request)
    {
        var userEmail = User.Identity!.Name;
        var updated = await userService.UpdateProfileAsync(userEmail, request.ExtractPersonalInfo(), HttpContext.RequestAborted);

        return Ok(updated);
    }
}
