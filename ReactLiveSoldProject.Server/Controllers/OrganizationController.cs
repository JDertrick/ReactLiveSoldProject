using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EmployeeOnly")] // Solo empleados regulares, no SuperAdmins
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(
            IOrganizationService organizationService,
            ILogger<OrganizationController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la organización del usuario autenticado
        /// </summary>
        [HttpGet("my-organization")]
        public async Task<ActionResult<OrganizationDto>> GetMyOrganization()
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var organization = await _organizationService.GetOrganizationByIdAsync(orgGuid);

                if (organization == null)
                    return NotFound(new { message = "Organización no encontrada" });

                return Ok(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's organization");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza la configuración de la organización del usuario
        /// Solo permite actualizar campos no sensibles (nombre, logo, email, customización)
        /// </summary>
        [HttpPut("my-organization")]
        public async Task<ActionResult<OrganizationDto>> UpdateMyOrganization(
            [FromBody] UpdateOrganizationSettingsDto dto)
        {
            try
            {
                _logger.LogInformation("Attempting to update organization settings");

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Datos inválidos", errors });
                }

                var organizationId = User.FindFirst("OrganizationId")?.Value;
                _logger.LogInformation("OrganizationId from token: {OrganizationId}", organizationId);

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    _logger.LogWarning("User not associated with organization");
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var organization = await _organizationService.UpdateOrganizationSettingsAsync(orgGuid, dto);
                _logger.LogInformation("Organization settings updated successfully");
                return Ok(organization);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Organization not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization settings");
                return StatusCode(500, new { message = "Error interno del servidor", detail = ex.Message });
            }
        }
    }
}
