using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class PaymentTermsController : BaseController
    {
        private readonly IPaymentTermsService _paymentTermsService;

        public PaymentTermsController(IPaymentTermsService paymentTermsService)
        {
            _paymentTermsService = paymentTermsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentTerms()
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var paymentTerms = await _paymentTermsService.GetPaymentTermsAsync(organizationId.Value);
                return Ok(paymentTerms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener términos de pago", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentTermsById(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var paymentTerms = await _paymentTermsService.GetPaymentTermsByIdAsync(id, organizationId.Value);
                if (paymentTerms == null)
                    return NotFound(new { message = "Términos de pago no encontrados" });

                return Ok(paymentTerms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener términos de pago", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentTerms([FromBody] CreatePaymentTermsDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var paymentTerms = await _paymentTermsService.CreatePaymentTermsAsync(organizationId.Value, dto);
                return CreatedAtAction(
                    nameof(GetPaymentTermsById),
                    new { id = paymentTerms.Id },
                    paymentTerms);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear términos de pago", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentTerms(Guid id, [FromBody] UpdatePaymentTermsDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var paymentTerms = await _paymentTermsService.UpdatePaymentTermsAsync(id, organizationId.Value, dto);
                return Ok(paymentTerms);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar términos de pago", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentTerms(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                await _paymentTermsService.DeletePaymentTermsAsync(id, organizationId.Value);
                return Ok(new { message = "Términos de pago eliminados exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar términos de pago", error = ex.Message });
            }
        }
    }
}
