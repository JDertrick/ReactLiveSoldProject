using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System.Security.Claims;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class SalesOrderController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ILogger<SalesOrderController> _logger;

        public SalesOrderController(
            ISalesOrderService salesOrderService,
            ILogger<SalesOrderController> logger)
        {
            _salesOrderService = salesOrderService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las órdenes de la organización
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<SalesOrderDto>>> GetSalesOrders([FromQuery] string? status = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var orders = await _salesOrderService.GetSalesOrdersByOrganizationAsync(organizationId.Value, status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales orders");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una orden por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesOrderDto>> GetSalesOrder(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var order = await _salesOrderService.GetSalesOrderByIdAsync(id, organizationId.Value);

                if (order == null)
                    return NotFound(new { message = "Orden no encontrada" });

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales order {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todas las órdenes de un cliente
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<SalesOrderDto>>> GetSalesOrdersByCustomer(Guid customerId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var orders = await _salesOrderService.GetSalesOrdersByCustomerIdAsync(customerId, organizationId.Value);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales orders for customer {CustomerId}", customerId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea una nueva orden en estado Draft
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SalesOrderDto>> CreateDraftOrder([FromBody] CreateSalesOrderDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var order = await _salesOrderService.CreateDraftOrderAsync(organizationId.Value, userId.Value, dto);

                return CreatedAtAction(
                    nameof(GetSalesOrder),
                    new { id = order.Id },
                    order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating draft order: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating draft order");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Agrega un item a una orden Draft
        /// </summary>
        [HttpPost("{id}/items")]
        public async Task<ActionResult<SalesOrderDto>> AddItemToOrder(Guid id, [FromBody] CreateSalesOrderItemDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var order = await _salesOrderService.AddItemToOrderAsync(id, organizationId.Value, dto);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation adding item to order {OrderId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to order {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un item de una orden Draft
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<ActionResult<SalesOrderDto>> RemoveItemFromOrder(Guid id, Guid itemId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var order = await _salesOrderService.RemoveItemFromOrderAsync(id, itemId, organizationId.Value);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation removing item from order {OrderId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from order {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Finaliza una orden: descuenta stock, descuenta wallet, cambia estado a Completed
        /// </summary>
        [HttpPost("{id}/finalize")]
        public async Task<ActionResult<SalesOrderDto>> FinalizeOrder(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var order = await _salesOrderService.FinalizeOrderAsync(id, organizationId.Value);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation finalizing order {OrderId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing order {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cancela una orden Draft
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<SalesOrderDto>> CancelOrder(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var order = await _salesOrderService.CancelOrderAsync(id, organizationId.Value);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation canceling order {OrderId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling order {OrderId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private Guid? GetOrganizationId()
        {
            var claim = User.FindFirst("OrganizationId");
            if (claim != null && Guid.TryParse(claim.Value, out var organizationId))
                return organizationId;
            return null;
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && Guid.TryParse(claim.Value, out var userId))
                return userId;
            return null;
        }
    }
}
