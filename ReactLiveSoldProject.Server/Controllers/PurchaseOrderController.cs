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
    public class PurchaseOrderController : BaseController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrders(
            [FromQuery] Guid? vendorId = null,
            [FromQuery] string? status = null)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var orders = await _purchaseOrderService.GetPurchaseOrdersAsync(
                    organizationId.Value,
                    vendorId,
                    status);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener órdenes de compra", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseOrder(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var order = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id, organizationId.Value);
                if (order == null)
                    return NotFound(new { message = "Orden de compra no encontrada" });

                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener orden de compra", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            if (organizationId == null || userId == null)
                return Unauthorized(new { message = "OrganizationId o UserId no encontrado en el token" });

            try
            {
                var order = await _purchaseOrderService.CreatePurchaseOrderAsync(
                    organizationId.Value,
                    userId.Value,
                    dto);

                return CreatedAtAction(
                    nameof(GetPurchaseOrder),
                    new { id = order.Id },
                    order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear orden de compra", error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdatePurchaseOrderStatus(Guid id, [FromBody] UpdateStatusDto dto)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var order = await _purchaseOrderService.UpdatePurchaseOrderStatusAsync(id, organizationId.Value, dto.Status);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar estado", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseOrder(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                await _purchaseOrderService.DeletePurchaseOrderAsync(id, organizationId.Value);
                return Ok(new { message = "Orden de compra eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar orden de compra", error = ex.Message });
            }
        }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
