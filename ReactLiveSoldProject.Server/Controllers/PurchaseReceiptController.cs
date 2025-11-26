using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.Server.Controllers.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

namespace ReactLiveSoldProject.Server.Controllers
{
    /// <summary>
    /// 📦 PurchaseReceiptController - Gestiona recepciones de compras
    ///
    /// ENDPOINTS CRÍTICOS:
    /// - POST /api/purchasereceipt/{id}/receive: Recibe mercancía y genera StockMovements + JournalEntry
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class PurchaseReceiptController : BaseController
    {
        private readonly IPurchaseReceiptService _purchaseReceiptService;

        public PurchaseReceiptController(IPurchaseReceiptService purchaseReceiptService)
        {
            _purchaseReceiptService = purchaseReceiptService;
        }

        /// <summary>
        /// Obtiene todas las recepciones de compra con filtros opcionales
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="status">Estado: Pending, Received, Invoiced</param>
        [HttpGet]
        public async Task<IActionResult> GetPurchaseReceipts(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var receipts = await _purchaseReceiptService.GetPurchaseReceiptsAsync(
                    organizationId.Value,
                    searchTerm,
                    status);

                return Ok(receipts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener recepciones de compra", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una recepción de compra por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseReceipt(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                var receipt = await _purchaseReceiptService.GetPurchaseReceiptByIdAsync(id, organizationId.Value);
                if (receipt == null)
                    return NotFound(new { message = "Recepción de compra no encontrada" });

                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener recepción de compra", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva recepción de compra (estado: Pending)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePurchaseReceipt([FromBody] CreatePurchaseReceiptDto dto)
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            if (organizationId == null || userId == null)
                return Unauthorized(new { message = "OrganizationId o UserId no encontrado en el token" });

            try
            {
                var receipt = await _purchaseReceiptService.CreatePurchaseReceiptAsync(
                    organizationId.Value,
                    userId.Value,
                    dto);

                return CreatedAtAction(
                    nameof(GetPurchaseReceipt),
                    new { id = receipt.Id },
                    receipt);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear recepción de compra", error = ex.Message });
            }
        }

        /// <summary>
        /// ⚡ ACCIÓN CRÍTICA: Recibe la mercancía
        ///
        /// Genera automáticamente:
        /// - StockMovement para cada item
        /// - StockBatch para FIFO costing
        /// - JournalEntry: DEBE Inventario / HABER Cuentas por Pagar
        /// </summary>
        [HttpPost("{id}/receive")]
        public async Task<IActionResult> ReceivePurchase(Guid id, [FromBody] ReceivePurchaseDto dto)
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();

            if (organizationId == null || userId == null)
                return Unauthorized(new { message = "OrganizationId o UserId no encontrado en el token" });

            // Asegurar que el ID del DTO coincida con el ID de la ruta
            dto.PurchaseReceiptId = id;

            try
            {
                var receipt = await _purchaseReceiptService.ReceivePurchaseAsync(
                    organizationId.Value,
                    userId.Value,
                    dto);

                return Ok(new
                {
                    message = "Recepción procesada exitosamente",
                    receipt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al procesar recepción", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una recepción de compra (solo si está en estado Pending)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseReceipt(Guid id)
        {
            var organizationId = GetOrganizationId();
            if (organizationId == null)
                return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

            try
            {
                await _purchaseReceiptService.DeletePurchaseReceiptAsync(id, organizationId.Value);
                return Ok(new { message = "Recepción de compra eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar recepción de compra", error = ex.Message });
            }
        }
    }
}
