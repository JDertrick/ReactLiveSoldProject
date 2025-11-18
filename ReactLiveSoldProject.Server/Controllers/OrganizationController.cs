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
        private readonly IFileService _fileService;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(
            IOrganizationService organizationService,
            IFileService fileService,
            ILogger<OrganizationController> logger)
        {
            _organizationService = organizationService;
            _fileService = fileService;
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

        /// <summary>
        /// Sube el logo de la organización
        /// </summary>
        [HttpPost("my-organization/logo")]
        public async Task<ActionResult<OrganizationDto>> UploadOrganizationLogo(IFormFile logo)
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                if (logo == null || logo.Length == 0)
                    return BadRequest(new { message = "No se ha proporcionado ninguna imagen" });

                if (!_fileService.IsValidImage(logo))
                    return BadRequest(new { message = "El archivo no es una imagen válida o excede el tamaño máximo permitido (5 MB)" });

                // Obtener la organización actual
                var existingOrg = await _organizationService.GetOrganizationByIdAsync(orgGuid);
                if (existingOrg == null)
                    return NotFound(new { message = "Organización no encontrada" });

                // Eliminar el logo anterior si existe
                if (!string.IsNullOrEmpty(existingOrg.LogoUrl))
                {
                    await _fileService.DeleteFileAsync(existingOrg.LogoUrl);
                }

                // Guardar el nuevo logo
                var logoUrl = await _fileService.SaveOrganizationLogoAsync(logo, orgGuid);

                // Actualizar la organización con la nueva URL del logo
                var updateDto = new UpdateOrganizationSettingsDto
                {
                    Name = existingOrg.Name,
                    LogoUrl = logoUrl,
                    PrimaryContactEmail = existingOrg.PrimaryContactEmail,
                    CustomizationSettings = existingOrg.CustomizationSettings
                };

                var updatedOrg = await _organizationService.UpdateOrganizationSettingsAsync(orgGuid, updateDto);

                _logger.LogInformation("Logo uploaded successfully for organization {OrganizationId}", orgGuid);
                return Ok(updatedOrg);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Organization not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation uploading logo: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo for organization");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
