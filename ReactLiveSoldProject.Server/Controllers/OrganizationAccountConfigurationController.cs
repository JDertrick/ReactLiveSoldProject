using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class OrganizationAccountConfigurationController : BaseController
    {
        private readonly IOrganizationAccountConfigurationService _configService;

        public OrganizationAccountConfigurationController(IOrganizationAccountConfigurationService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Obtiene la configuración de cuentas contables de la organización
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConfiguration()
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var config = await _configService.GetConfigurationAsync(organizationId.Value);
                if (config == null)
                {
                    // Si no existe configuración, retornar un objeto vacío
                    return Ok(new OrganizationAccountConfigurationDto
                    {
                        Id = Guid.Empty,
                        OrganizationId = organizationId.Value,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la configuración", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea o actualiza la configuración de cuentas contables
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> CreateOrUpdateConfiguration([FromBody] UpdateOrganizationAccountConfigurationDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var config = await _configService.CreateOrUpdateConfigurationAsync(organizationId.Value, dto);
                return Ok(config);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al guardar la configuración", error = ex.Message });
            }
        }
    }
}
