using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.Server.Controllers.Base;

namespace ReactLiveSoldProject.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVendorController : BaseController
    {
        private readonly IProductVendorService _productVendorService;

        public ProductVendorController(IProductVendorService productVendorService)
        {
            _productVendorService = productVendorService;
        }

        /// <summary>
        /// Obtiene todas las relaciones producto-proveedor con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ProductVendorDto>>> GetProductVendors(
            [FromQuery] Guid? productId = null,
            [FromQuery] Guid? vendorId = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var productVendors = await _productVendorService.GetProductVendorsAsync(organizationId.Value, productId, vendorId);
                return Ok(productVendors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una relación producto-proveedor por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductVendorDto>> GetProductVendorById(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var productVendor = await _productVendorService.GetProductVendorByIdAsync(id, organizationId.Value);

                if (productVendor == null)
                    return NotFound(new { message = "Relación producto-proveedor no encontrada" });

                return Ok(productVendor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva relación producto-proveedor
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductVendorDto>> CreateProductVendor([FromBody] CreateProductVendorDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var productVendor = await _productVendorService.CreateProductVendorAsync(organizationId.Value, dto);
                return CreatedAtAction(nameof(GetProductVendorById), new { id = productVendor.Id }, productVendor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una relación producto-proveedor
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductVendorDto>> UpdateProductVendor(Guid id, [FromBody] UpdateProductVendorDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                var productVendor = await _productVendorService.UpdateProductVendorAsync(id, organizationId.Value, dto);
                return Ok(productVendor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una relación producto-proveedor
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProductVendor(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                await _productVendorService.DeleteProductVendorAsync(id, organizationId.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
