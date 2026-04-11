using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Lib.BusinessLogic.Admin.MigrateDb;

namespace PeopleHub.Controllers
{
    using MigrateDbRequest = Request;
    
    [AllowAnonymous]
    [Route("admin/db")]
    public class DbController : Controller
    {
        private readonly IMediator _mediator;
        
        public DbController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("migrate")]
        public async Task<IActionResult> MigrateDb()
        {
            // TODO перенести это в какой-нито sandbox
            await _mediator.Send(new MigrateDbRequest(), HttpContext.RequestAborted);

            return Ok("Ok");
        }
    }
}