using System.Security.Claims;
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
public sealed class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly long _userId;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
        _userId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet]
    public async Task<ActionResult<PersonalInfo>> Get() => Ok(await _userService.GetProfileAsync(_userId, HttpContext.RequestAborted));

    [HttpPut]
    public async Task<ActionResult<PersonalInfo>> Update([FromBody] UpdateMyProfileRequest request) => 
        Ok(await _userService.UpdateProfileAsync(_userId, request.ExtractPersonalInfo(), HttpContext.RequestAborted));
}
