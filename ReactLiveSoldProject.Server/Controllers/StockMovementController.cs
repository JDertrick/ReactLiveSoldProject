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
    public class StockMovementController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;
        private readonly ILogger<StockMovementController> _logger;

        public StockMovementController(
            IStockMovementService stockMovementService,
            ILogger<StockMovementController> logger)
        {
            _stockMovementService = stockMovementService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el historial de movimientos de una variante específica
        /// </summary>
        [HttpGet("variant/{productVariantId}")]
        public async Task<ActionResult<List<StockMovementDto>>> GetMovementsByVariant(Guid productVariantId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var movements = await _stockMovementService.GetMovementsByVariantAsync(productVariantId, organizationId.Value);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock movements for variant {VariantId}", productVariantId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los movimientos de inventario de la organización
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<StockMovementDto>>> GetMovementsByOrganization(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var movements = await _stockMovementService.GetMovementsByOrganizationAsync(organizationId.Value, fromDate, toDate);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock movements for organization");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un movimiento de inventario manual (ajustes, compras, pérdidas, etc.)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<StockMovementDto>> CreateMovement([FromBody] CreateStockMovementDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var movement = await _stockMovementService.CreateMovementAsync(organizationId.Value, userId.Value, dto);
                return CreatedAtAction(nameof(GetMovementsByVariant), new { productVariantId = movement.ProductVariantId }, movement);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Entity not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating stock movement: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stock movement");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el stock actual de una variante
        /// </summary>
        [HttpGet("variant/{productVariantId}/current-stock")]
        public async Task<ActionResult<int>> GetCurrentStock(Guid productVariantId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var stock = await _stockMovementService.GetCurrentStockAsync(productVariantId, organizationId.Value);
                return Ok(new { stock });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current stock for variant {VariantId}", productVariantId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Postea un movimiento de inventario, actualizando el stock y el costo promedio
        /// </summary>
        [HttpPost("{movementId}/post")]
        public async Task<ActionResult<StockMovementDto>> PostMovement(Guid movementId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var movement = await _stockMovementService.PostMovementAsync(movementId, organizationId.Value, userId.Value);
                return Ok(movement);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Movement not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation posting movement: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting stock movement {MovementId}", movementId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Despostea un movimiento de inventario (solo si es el último movimiento posteado)
        /// </summary>
        [HttpPost("{movementId}/unpost")]
        public async Task<ActionResult<StockMovementDto>> UnpostMovement(Guid movementId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var movement = await _stockMovementService.UnpostMovementAsync(movementId, organizationId.Value);
                return Ok(movement);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Movement not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation unposting movement: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unposting stock movement {MovementId}", movementId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Rechaza un movimiento de inventario que está en estado de borrador.
        /// </summary>
        [HttpPost("{movementId}/reject")]
        public async Task<ActionResult<StockMovementDto>> RejectMovement(Guid movementId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var movement = await _stockMovementService.RejectMovementAsync(movementId, organizationId.Value, userId.Value);
                return Ok(movement);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Movement not found: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation rejecting movement: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized reject movement attempt: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting stock movement {MovementId}", movementId);
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
