using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Shared.BusinessLogic.Admin;

namespace PeopleHub.Controllers
{
    [AllowAnonymous]
    [Route("admin/db")]
    public class DbController(IMediator mediator) : Controller
    {

        [HttpGet("migrate")]
        public async Task<IActionResult> MigrateDb()
        {
            // TODO перенести это в какой-нито sandbox
            await mediator.Send(new MigrateDbRequest(), HttpContext.RequestAborted);

            return Ok("Ok");
        }
    }
}
