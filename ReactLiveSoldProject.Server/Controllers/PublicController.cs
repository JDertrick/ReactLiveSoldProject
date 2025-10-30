using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Services;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<PublicController> _logger;

        public PublicController(
            IOrganizationService organizationService,
            ILogger<PublicController> logger)
        {
            _organizationService = organizationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene información pública de una organización por su slug
        /// ENDPOINT PÚBLICO - No requiere autenticación
        /// Solo devuelve: name y logoUrl (información segura)
        /// </summary>
        [HttpGet("organization-by-slug/{slug}")]
        public async Task<ActionResult<OrganizationPublicDto>> GetOrganizationBySlug(string slug)
        {
            try
            {
                var organization = await _organizationService.GetOrganizationBySlugAsync(slug);

                if (organization == null)
                    return NotFound(new { message = "Organización no encontrada" });

                return Ok(organization);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization by slug: {Slug}", slug);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
