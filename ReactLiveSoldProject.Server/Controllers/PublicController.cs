using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<PublicController> _logger;

        public PublicController(
            IOrganizationService organizationService,
            ICustomerService customerService,
            ILogger<PublicController> logger)
        {
            _organizationService = organizationService;
            _customerService = customerService;
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

        /// <summary>
        /// Registra un nuevo customer desde el portal público
        /// ENDPOINT PÚBLICO - No requiere autenticación
        /// </summary>
        [HttpPost("register-customer")]
        public async Task<ActionResult<CustomerDto>> RegisterCustomer([FromBody] RegisterCustomerDto dto)
        {
            try
            {
                var customer = await _customerService.RegisterCustomerAsync(dto);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Organization not found: {Slug}", dto.OrganizationSlug);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation registering customer: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering customer for organization: {Slug}", dto.OrganizationSlug);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
