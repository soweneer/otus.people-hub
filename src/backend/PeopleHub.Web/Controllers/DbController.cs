using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Controllers
{
    [AllowAnonymous]
    [Route("api/admin/db")]
    public class DbController(IDbMigrator dbMigrator) : Controller
    {

        [HttpGet("migrate")]
        public async Task<IActionResult> MigrateDb()
        {
            // TODO перенести это в какой-нито sandbox
            await dbMigrator.MigrateAsync();

            return Ok("Ok");
        }
    }
}
