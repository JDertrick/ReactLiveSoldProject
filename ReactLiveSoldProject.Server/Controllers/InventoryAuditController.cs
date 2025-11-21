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
    public class InventoryAuditController : ControllerBase
    {
        private readonly IInventoryAuditService _auditService;
        private readonly ILogger<InventoryAuditController> _logger;

        public InventoryAuditController(
            IInventoryAuditService auditService,
            ILogger<InventoryAuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las auditorías de la organización
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<InventoryAuditDto>>> GetAudits([FromQuery] string? status = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var audits = await _auditService.GetAuditsByOrganizationAsync(organizationId.Value, status);
                return Ok(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audits");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene una auditoría por ID con todos sus items
        /// </summary>
        [HttpGet("{auditId}")]
        public async Task<ActionResult<InventoryAuditDetailDto>> GetAuditById(Guid auditId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var audit = await _auditService.GetAuditByIdAsync(auditId, organizationId.Value);
                if (audit == null)
                    return NotFound(new { message = "Auditoría no encontrada" });

                return Ok(audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea una nueva auditoría y toma snapshot del inventario
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<InventoryAuditDto>> CreateAudit([FromBody] CreateInventoryAuditDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var audit = await _auditService.CreateAuditAsync(organizationId.Value, userId.Value, dto);
                return CreatedAtAction(nameof(GetAuditById), new { auditId = audit.Id }, audit);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating audit: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Inicia el proceso de conteo de una auditoría
        /// </summary>
        [HttpPost("{auditId}/start")]
        public async Task<ActionResult<InventoryAuditDto>> StartAudit(Guid auditId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var audit = await _auditService.StartAuditAsync(auditId, organizationId.Value, userId.Value);
                return Ok(audit);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene los items para conteo ciego (sin stock teórico)
        /// </summary>
        [HttpGet("{auditId}/blind-count")]
        public async Task<ActionResult<List<BlindCountItemDto>>> GetBlindCountItems(Guid auditId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var items = await _auditService.GetBlindCountItemsAsync(auditId, organizationId.Value);
                return Ok(items);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blind count items for audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los items de la auditoría (con stock teórico, para revisión)
        /// </summary>
        [HttpGet("{auditId}/items")]
        public async Task<ActionResult<List<InventoryAuditItemDto>>> GetAuditItems(Guid auditId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var items = await _auditService.GetAuditItemsAsync(auditId, organizationId.Value);
                return Ok(items);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit items for audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza el conteo de un item
        /// </summary>
        [HttpPut("{auditId}/count")]
        public async Task<ActionResult<InventoryAuditItemDto>> UpdateCount(Guid auditId, [FromBody] UpdateAuditCountDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var item = await _auditService.UpdateCountAsync(auditId, organizationId.Value, userId.Value, dto);
                return Ok(item);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating count for audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza múltiples conteos a la vez
        /// </summary>
        [HttpPut("{auditId}/bulk-count")]
        public async Task<ActionResult<List<InventoryAuditItemDto>>> BulkUpdateCount(Guid auditId, [FromBody] BulkUpdateAuditCountDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var items = await _auditService.BulkUpdateCountAsync(auditId, organizationId.Value, userId.Value, dto);
                return Ok(items);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating counts for audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el resumen de la auditoría con estadísticas
        /// </summary>
        [HttpGet("{auditId}/summary")]
        public async Task<ActionResult<InventoryAuditSummaryDto>> GetAuditSummary(Guid auditId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var summary = await _auditService.GetAuditSummaryAsync(auditId, organizationId.Value);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting summary for audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Completa la auditoría y genera movimientos de ajuste
        /// </summary>
        [HttpPost("{auditId}/complete")]
        public async Task<ActionResult<InventoryAuditDto>> CompleteAudit(Guid auditId, [FromBody] CompleteAuditDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var audit = await _auditService.CompleteAuditAsync(auditId, organizationId.Value, userId.Value, dto);
                return Ok(audit);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing audit {AuditId}", auditId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cancela una auditoría
        /// </summary>
        [HttpPost("{auditId}/cancel")]
        public async Task<ActionResult<InventoryAuditDto>> CancelAudit(Guid auditId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new { message = "UserId no encontrado en el token" });

                var audit = await _auditService.CancelAuditAsync(auditId, organizationId.Value, userId.Value);
                return Ok(audit);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling audit {AuditId}", auditId);
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
