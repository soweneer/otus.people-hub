using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Model;
using PeopleHub.Extensions;
using PeopleHub.Model;

namespace PeopleHub.Controllers.Api;

[ApiController]
[Route("api/profile")]
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ProfileApiController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PersonalInfo>> Get()
    {
        return Ok(await userService.GetProfileAsync(HttpContext.RequestAborted));
    }

    [HttpPut]
    public async Task<ActionResult<PersonalInfo>> Update([FromBody] UpdateMyProfileRequest request)
    {
        var updated = await userService.UpdateProfileAsync(request.ExtractPersonalInfo(), HttpContext.RequestAborted);

        return Ok(updated);
    }
}
