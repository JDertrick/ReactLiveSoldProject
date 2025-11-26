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
    public class VendorInvoiceController : BaseController
    {
        private readonly IVendorInvoiceService _vendorInvoiceService;

        public VendorInvoiceController(IVendorInvoiceService vendorInvoiceService)
        {
            _vendorInvoiceService = vendorInvoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorInvoices(
            [FromQuery] Guid? vendorId = null,
            [FromQuery] string? status = null)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var invoices = await _vendorInvoiceService.GetVendorInvoicesAsync(
                    organizationId.Value,
                    vendorId,
                    status);

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener facturas", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVendorInvoice(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var invoice = await _vendorInvoiceService.GetVendorInvoiceByIdAsync(id, organizationId.Value);
                if (invoice == null)
                    return NotFound(new { message = "Factura no encontrada" });

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener factura", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendorInvoice([FromBody] CreateVendorInvoiceDto dto)
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            if (organizationId == null || userId == null)
                return Unauthorized(new { message = "OrganizationId o UserId no encontrado en el token" });

            try
            {
                var invoice = await _vendorInvoiceService.CreateVendorInvoiceAsync(
                    organizationId.Value,
                    userId.Value,
                    dto);

                return CreatedAtAction(
                    nameof(GetVendorInvoice),
                    new { id = invoice.Id },
                    invoice);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear factura", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVendorInvoice(Guid id, [FromBody] CreateVendorInvoiceDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var invoice = await _vendorInvoiceService.UpdateVendorInvoiceAsync(id, organizationId.Value, dto);
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar factura", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendorInvoice(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                await _vendorInvoiceService.DeleteVendorInvoiceAsync(id, organizationId.Value);
                return Ok(new { message = "Factura eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar factura", error = ex.Message });
            }
        }
    }
}
