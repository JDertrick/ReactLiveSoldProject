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
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        private readonly ILogger<VendorController> _logger;

        public VendorController(
            IVendorService vendorService,
            ILogger<VendorController> logger)
        {
            _vendorService = vendorService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los proveedores de la organización del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<VendorDto>>> GetVendors([FromQuery] string? searchTerm, [FromQuery] string? status)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var vendors = await _vendorService.GetVendorsByOrganizationAsync(organizationId.Value, searchTerm, status);

                return Ok(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vendors");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene un proveedor por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorDto>> GetVendor(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var vendor = await _vendorService.GetVendorByIdAsync(id, organizationId.Value);

                if (vendor == null)
                    return NotFound(new { message = "Proveedor no encontrado" });

                return Ok(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vendor {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca proveedores por código o información del contacto
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<VendorDto>>> SearchVendors([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "El término de búsqueda es requerido" });

                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var vendors = await _vendorService.SearchVendorsAsync(organizationId.Value, q);
                return Ok(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vendors with term: {SearchTerm}", q);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo proveedor
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VendorDto>> CreateVendor([FromBody] CreateVendorDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var vendor = await _vendorService.CreateVendorAsync(organizationId.Value, dto);

                return CreatedAtAction(
                    nameof(GetVendor),
                    new { id = vendor.Id },
                    vendor);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating vendor: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vendor");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un proveedor existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<VendorDto>> UpdateVendor(Guid id, [FromBody] UpdateVendorDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var vendor = await _vendorService.UpdateVendorAsync(id, organizationId.Value, dto);
                return Ok(vendor);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vendor not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation updating vendor {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vendor {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un proveedor (solo si no tiene órdenes de compra)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<ActionResult> DeleteVendor(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                await _vendorService.DeleteVendorAsync(id, organizationId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Vendor not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete vendor {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vendor {Id}", id);
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
    }
}
