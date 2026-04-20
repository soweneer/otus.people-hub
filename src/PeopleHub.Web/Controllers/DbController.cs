using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;

namespace PeopleHub.Controllers
{
    [AllowAnonymous]
    [Route("admin/db")]
    public class DbController(IAdminRepository adminRepository) : Controller
    {

        [HttpGet("migrate")]
        public async Task<IActionResult> MigrateDb()
        {
            // TODO перенести это в какой-нито sandbox
            await adminRepository.MigrateAsync();

            return Ok("Ok");
        }
    }
}
