using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            IOrganizationService organizationService,
            ILogger<SuperAdminController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las organizaciones (solo SuperAdmin)
        /// </summary>
        [HttpGet("organizations")]
        public async Task<ActionResult<List<OrganizationDto>>> GetAllOrganizations()
        {
            try
            {
                var organizations = await _organizationService.GetAllOrganizationsAsync();
                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all organizations");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una organización por ID (solo SuperAdmin)
        /// </summary>
        [HttpGet("organizations/{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganizationById(Guid id)
        {
            try
            {
                var organization = await _organizationService.GetOrganizationByIdAsync(id);

                if (organization == null)
                    return NotFound(new { message = "Organización no encontrada" });

                return Ok(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea una nueva organización (solo SuperAdmin)
        /// </summary>
        [HttpPost("organizations")]
        public async Task<ActionResult<OrganizationDto>> CreateOrganization(
            [FromBody] CreateOrganizationDto dto)
        {
            try
            {
                var organization = await _organizationService.CreateOrganizationAsync(dto);

                return CreatedAtAction(
                    nameof(GetOrganizationById),
                    new { id = organization.Id },
                    organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza una organización existente (solo SuperAdmin)
        /// </summary>
        [HttpPut("organizations/{id}")]
        public async Task<ActionResult<OrganizationDto>> UpdateOrganization(
            Guid id,
            [FromBody] CreateOrganizationDto dto)
        {
            try
            {
                var organization = await _organizationService.UpdateOrganizationAsync(id, dto);
                return Ok(organization);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Organization not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina una organización (solo SuperAdmin)
        /// CUIDADO: Solo funciona si no tiene datos relacionados
        /// </summary>
        [HttpDelete("organizations/{id}")]
        public async Task<ActionResult> DeleteOrganization(Guid id)
        {
            try
            {
                await _organizationService.DeleteOrganizationAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Organization not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete organization {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
