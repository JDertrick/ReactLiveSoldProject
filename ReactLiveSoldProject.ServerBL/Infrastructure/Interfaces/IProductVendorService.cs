using ReactLiveSoldProject.ServerBL.DTOs.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IProductVendorService
    {
        Task<List<ProductVendorDto>> GetProductVendorsAsync(Guid organizationId, Guid? productId = null, Guid? vendorId = null);
        Task<ProductVendorDto?> GetProductVendorByIdAsync(Guid productVendorId, Guid organizationId);
        Task<ProductVendorDto> CreateProductVendorAsync(Guid organizationId, CreateProductVendorDto dto);
        Task<ProductVendorDto> UpdateProductVendorAsync(Guid productVendorId, Guid organizationId, UpdateProductVendorDto dto);
        Task DeleteProductVendorAsync(Guid productVendorId, Guid organizationId);
    }
}
