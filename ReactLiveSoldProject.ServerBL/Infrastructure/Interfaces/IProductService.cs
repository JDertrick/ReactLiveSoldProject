using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Obtiene todos los productos de una organización
        /// </summary>
        Task<List<ProductDto>> GetProductsByOrganizationAsync(Guid organizationId, bool includeUnpublished = false);

        /// <summary>
        /// Obtiene un producto por ID
        /// </summary>
        Task<ProductDto?> GetProductByIdAsync(Guid productId, Guid organizationId);

        /// <summary>
        /// Busca productos por nombre dentro de una organización
        /// </summary>
        Task<List<ProductDto>> SearchProductsAsync(Guid organizationId, string searchTerm);

        /// <summary>
        /// Crea un nuevo producto con sus variantes
        /// </summary>
        Task<ProductDto> CreateProductAsync(Guid organizationId, CreateProductDto dto);

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        Task<ProductDto> UpdateProductAsync(Guid productId, Guid organizationId, UpdateProductDto dto);

        /// <summary>
        /// Elimina un producto (solo si no tiene items en órdenes)
        /// </summary>
        Task DeleteProductAsync(Guid productId, Guid organizationId);

        /// <summary>
        /// Obtiene todos los tags de una organización
        /// </summary>
        Task<List<TagDto>> GetTagsByOrganizationAsync(Guid organizationId);

        /// <summary>
        /// Crea un nuevo tag
        /// </summary>
        Task<TagDto> CreateTagAsync(Guid organizationId, string name);

        /// <summary>
        /// Agrega una variante a un producto existente
        /// </summary>
        Task<ProductVariantDto> AddVariantAsync(Guid productId, Guid organizationId, CreateProductVariantDto dto);

        /// <summary>
        /// Actualiza una variante existente
        /// </summary>
        Task<ProductVariantDto> UpdateVariantAsync(Guid variantId, Guid organizationId, CreateProductVariantDto dto);

        /// <summary>
        /// Elimina una variante (solo si no tiene items en órdenes)
        /// </summary>
        Task DeleteVariantAsync(Guid variantId, Guid organizationId);
    }
}
