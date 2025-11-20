using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "EmployeeOnly")]
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _taxService;
        private readonly ILogger<TaxController> _logger;

        public TaxController(
            ITaxService taxService,
            ILogger<TaxController> logger)
        {
            _taxService = taxService;
            _logger = logger;
        }

        // ==================== CONFIGURACIÓN DE IMPUESTOS ====================

        /// <summary>
        /// Obtiene la configuración de impuestos de la organización
        /// </summary>
        [HttpGet("configuration")]
        public async Task<ActionResult<TaxConfigurationDto>> GetTaxConfiguration()
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var config = await _taxService.GetTaxConfigurationAsync(orgGuid);
                return Ok(config);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax configuration");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza la configuración de impuestos de la organización
        /// </summary>
        [HttpPut("configuration")]
        public async Task<ActionResult> UpdateTaxConfiguration([FromBody] UpdateTaxConfigurationDto dto)
        {
            try
            {
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

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                await _taxService.UpdateTaxConfigurationAsync(orgGuid, dto);
                return Ok(new { message = "Configuración de impuestos actualizada correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax configuration");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // ==================== GESTIÓN DE TASAS ====================

        /// <summary>
        /// Obtiene todas las tasas de impuesto de la organización
        /// </summary>
        [HttpGet("rates")]
        public async Task<ActionResult<List<TaxRateDto>>> GetTaxRates()
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var rates = await _taxService.GetTaxRatesAsync(orgGuid);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax rates");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una tasa de impuesto por ID
        /// </summary>
        [HttpGet("rates/{taxRateId}")]
        public async Task<ActionResult<TaxRateDto>> GetTaxRateById(Guid taxRateId)
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var rate = await _taxService.GetTaxRateByIdAsync(taxRateId, orgGuid);

                if (rate == null)
                    return NotFound(new { message = "Tasa de impuesto no encontrada" });

                return Ok(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax rate by ID");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene la tasa de impuesto por defecto
        /// </summary>
        [HttpGet("rates/default")]
        public async Task<ActionResult<TaxRateDto>> GetDefaultTaxRate()
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var rate = await _taxService.GetDefaultTaxRateAsync(orgGuid);

                if (rate == null)
                    return NotFound(new { message = "No hay tasa de impuesto por defecto configurada" });

                return Ok(rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default tax rate");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea una nueva tasa de impuesto
        /// </summary>
        [HttpPost("rates")]
        public async Task<ActionResult<TaxRateDto>> CreateTaxRate([FromBody] CreateTaxRateDto dto)
        {
            try
            {
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

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var newRate = await _taxService.CreateTaxRateAsync(orgGuid, dto);
                return CreatedAtAction(nameof(GetTaxRateById), new { taxRateId = newRate.Id }, newRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tax rate");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza una tasa de impuesto existente
        /// </summary>
        [HttpPut("rates/{taxRateId}")]
        public async Task<ActionResult<TaxRateDto>> UpdateTaxRate(Guid taxRateId, [FromBody] UpdateTaxRateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { message = "Datos inválidos", errors });
                }

                if (dto.Id != taxRateId)
                {
                    return BadRequest(new { message = "El ID de la URL no coincide con el ID del DTO" });
                }

                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var updatedRate = await _taxService.UpdateTaxRateAsync(orgGuid, dto);
                return Ok(updatedRate);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tax rate not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tax rate");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina una tasa de impuesto
        /// </summary>
        [HttpDelete("rates/{taxRateId}")]
        public async Task<ActionResult> DeleteTaxRate(Guid taxRateId)
        {
            try
            {
                var organizationId = User.FindFirst("OrganizationId")?.Value;

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                await _taxService.DeleteTaxRateAsync(taxRateId, orgGuid);
                return Ok(new { message = "Tasa de impuesto eliminada correctamente" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tax rate not found");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete tax rate in use");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tax rate");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // ==================== CÁLCULOS DE IMPUESTOS ====================

        /// <summary>
        /// Calcula el impuesto para un monto dado
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<TaxCalculationResult>> CalculateTax([FromBody] TaxCalculationRequest request)
        {
            try
            {
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

                if (string.IsNullOrEmpty(organizationId) || !Guid.TryParse(organizationId, out var orgGuid))
                {
                    return BadRequest(new { message = "Usuario no asociado a una organización" });
                }

                var result = await _taxService.CalculateTaxAsync(
                    orgGuid,
                    request.Amount,
                    request.TaxRateId,
                    request.PriceIncludesTax);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    // DTO auxiliar para el cálculo de impuestos
    public class TaxCalculationRequest
    {
        public decimal Amount { get; set; }
        public Guid? TaxRateId { get; set; }
        public bool PriceIncludesTax { get; set; } = true;
    }
}
