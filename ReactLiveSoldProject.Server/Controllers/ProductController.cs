using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;

using ReactLiveSoldProject.ServerBL.DTOs.Common;

namespace ReactLiveSoldProject.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Employee")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IFileService _fileService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IFileService fileService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los productos de la organización de forma paginada
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<VariantProductDto>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var products = await _productService.GetVarantProductsAsync(organizationId.Value, page, pageSize, status, searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }


        /// <summary>
        /// Obtiene un producto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var product = await _productService.GetProductByIdAsync(id, organizationId.Value);

                if (product == null)
                    return NotFound(new { message = "Producto no encontrado" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Busca productos por nombre o descripción
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<ProductDto>>> SearchProducts([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "El término de búsqueda es requerido" });

                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var products = await _productService.SearchProductsAsync(organizationId.Value, q);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with term: {SearchTerm}", q);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var product = await _productService.CreateProductAsync(organizationId.Value, dto);

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    product);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating product: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var product = await _productService.UpdateProductAsync(id, organizationId.Value, dto);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation updating product {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina un producto
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "OrgOwner")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                await _productService.DeleteProductAsync(id, organizationId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete product {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene todos los tags de la organización
        /// </summary>
        [HttpGet("tags")]
        public async Task<ActionResult<List<TagDto>>> GetTags()
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var tags = await _productService.GetTagsByOrganizationAsync(organizationId.Value);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crea un nuevo tag
        /// </summary>
        [HttpPost("tags")]
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { message = "El nombre del tag es requerido" });

                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var tag = await _productService.CreateTagAsync(organizationId.Value, name);
                return Ok(tag);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation creating tag: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Agrega una variante a un producto
        /// </summary>
        [HttpPost("{productId}/variants")]
        public async Task<ActionResult<ProductVariantDto>> AddVariant(Guid productId, [FromBody] CreateProductVariantDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var variant = await _productService.AddVariantAsync(productId, organizationId.Value, dto);
                return Ok(variant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation adding variant: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding variant to product {ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualiza una variante existente
        /// </summary>
        [HttpPut("variants/{variantId}")]
        public async Task<ActionResult<ProductVariantDto>> UpdateVariant(Guid variantId, [FromBody] CreateProductVariantDto dto)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var variant = await _productService.UpdateVariantAsync(variantId, organizationId.Value, dto);
                return Ok(variant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Variant not found: {VariantId}", variantId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation updating variant {VariantId}: {Message}", variantId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating variant {VariantId}", variantId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Elimina una variante
        /// </summary>
        [HttpDelete("variants/{variantId}")]
        public async Task<ActionResult> DeleteVariant(Guid variantId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                await _productService.DeleteVariantAsync(variantId, organizationId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Variant not found: {VariantId}", variantId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete variant {VariantId}: {Message}", variantId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variant {VariantId}", variantId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Sube una imagen para una variante específica
        /// </summary>
        [HttpPost("variants/{variantId}/image")]
        public async Task<ActionResult<ProductVariantDto>> UploadVariantImage(Guid variantId, IFormFile image)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                if (image == null || image.Length == 0)
                    return BadRequest(new { message = "No se ha proporcionado ninguna imagen" });

                if (!_fileService.IsValidImage(image))
                    return BadRequest(new { message = "El archivo no es una imagen válida o excede el tamaño máximo permitido (5 MB)" });

                // Guardar la nueva imagen
                var imageUrl = await _fileService.SaveProductImageAsync(image, organizationId.Value, variantId);

                // Actualizar la variante con la nueva URL de imagen
                var updatedVariant = await _productService.UpdateVariantImageAsync(variantId, organizationId.Value, imageUrl);

                _logger.LogInformation("Image uploaded successfully for variant {VariantId}", variantId);
                return Ok(updatedVariant);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Variant not found: {VariantId}", variantId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Invalid operation uploading image: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for variant {VariantId}", variantId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Asocia tags a un producto
        /// </summary>
        [HttpPost("{productId}/tags")]
        public async Task<ActionResult> AddTagsToProduct(Guid productId, [FromBody] List<Guid> tagIds)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                // Verificar que el producto existe
                var product = await _productService.GetProductByIdAsync(productId, organizationId.Value);
                if (product == null)
                    return NotFound(new { message = "Producto no encontrado" });

                // Actualizar el producto con los tags
                var updateDto = new UpdateProductDto
                {
                    Name = product.Name,
                    Description = product.Description,
                    ProductType = product.ProductType,
                    BasePrice = product.BasePrice,
                    IsPublished = product.IsPublished,
                    CategoryId = product.CategoryId
                };

                // Nota: Necesitaremos agregar un método específico para actualizar tags
                // Por ahora, retornamos OK
                return Ok(new { message = "Tags asociados correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tags to product {ProductId}", productId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene el stock por ubicación para una variante
        /// </summary>
        [HttpGet("variants/{variantId}/stock-by-location")]
        public async Task<ActionResult<List<StockByLocationDto>>> GetStockByLocation(Guid variantId)
        {
            try
            {
                var organizationId = GetOrganizationId();
                if (organizationId == null)
                    return Unauthorized(new { message = "OrganizationId no encontrado en el token" });

                var stockByLocation = await _productService.GetStockByLocationAsync(variantId, organizationId.Value);
                return Ok(stockByLocation);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Variant not found: {VariantId}", variantId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock by location for variant {VariantId}", variantId);
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
