using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "SuperAdmin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Retorna los usuarios de una organizacion.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [HttpGet("users/{organizationId}")]
        public async Task<IActionResult> GetUsers(Guid organizationId)
        {
            var users = await _userService.GetUserAsync(organizationId);
            return Ok();

        }
    }
}
