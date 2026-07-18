using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Infrastructure.Db;

namespace PeopleHub.Controllers;

[AllowAnonymous]
[Route("api/admin/db")]
public sealed class DbController(IDbMigrator dbMigrator) : ControllerBase
{
    [HttpGet("migrate")]
    public async Task<IActionResult> MigrateDb()
    {
        // TODO перенести это в какой-нито sandbox
        await dbMigrator.MigrateAsync();

        return Ok("Ok");
    }
}
