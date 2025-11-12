using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login para empleados (Users: Seller, Owner, SuperAdmin)
        /// </summary>
        [HttpPost("employee-login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> EmployeeLogin([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.EmployeeLoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed employee login attempt for email: {Email}", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during employee login for email: {Email}", request.Email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Login para clientes del portal
        /// </summary>
        [HttpPost("portal/login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> CustomerPortalLogin(
            [FromBody] CustomerPortalLoginRequestDto request)
        {
            try
            {
                var response = await _authService.CustomerPortalLoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    "Failed customer login attempt for email: {Email}, org: {Org}",
                    request.Email,
                    request.OrganizationSlug);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error during customer login for email: {Email}, org: {Org}",
                    request.Email,
                    request.OrganizationSlug);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado (empleado o cliente)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetMyProfile()
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role == "Customer")
                {
                    // Es un cliente
                    var customerIdClaim = User.FindFirst("CustomerId")?.Value;
                    if (customerIdClaim == null)
                        return Unauthorized(new { message = "Token inválido" });

                    var customerId = Guid.Parse(customerIdClaim);
                    var profile = await _authService.GetCustomerProfileAsync(customerId);
                    return Ok(profile);
                }
                else
                {
                    // Es un empleado (Seller, Owner, SuperAdmin)
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim == null)
                        return Unauthorized(new { message = "Token inválido" });

                    var userId = Guid.Parse(userIdClaim);
                    var profile = await _authService.GetEmployeeProfileAsync(userId);
                    return Ok(profile);
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Profile not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
