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
    private long UserId => User.GetUserId();

    [HttpGet]
    public async Task<ActionResult<PersonalInfo>> Get() => Ok(await userService.GetProfileAsync(UserId, HttpContext.RequestAborted));

    [HttpPut]
    public async Task<ActionResult<PersonalInfo>> Update([FromBody] UpdateMyProfileRequest request) => 
        Ok(await userService.UpdateProfileAsync(UserId, request.ExtractPersonalInfo(), HttpContext.RequestAborted));
}
