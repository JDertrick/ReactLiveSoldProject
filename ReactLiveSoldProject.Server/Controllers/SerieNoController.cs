using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación, pero la autorización es por endpoint
    public class SerieNoController : BaseController
    {
        [HttpGet]
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> GetSerieNoAsync()
        {
            var organizationId = GetOrganizationId();
            return Ok();
        }
    }
}