using ReactLiveSoldProject.ServerBL.DTOs;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IVendorService
    {
        /// <summary>
        /// Obtiene todos los proveedores de una organización
        /// </summary>
        Task<List<VendorDto>> GetVendorsByOrganizationAsync(Guid organizationId);

        /// <summary>
        /// Obtiene un proveedor por ID (validando que pertenezca a la organización)
        /// </summary>
        Task<VendorDto?> GetVendorByIdAsync(Guid vendorId, Guid organizationId);

        /// <summary>
        /// Busca proveedores por código o información del contacto dentro de una organización
        /// </summary>
        Task<List<VendorDto>> SearchVendorsAsync(Guid organizationId, string searchTerm);

        /// <summary>
        /// Crea un nuevo proveedor
        /// </summary>
        Task<VendorDto> CreateVendorAsync(Guid organizationId, CreateVendorDto dto);

        /// <summary>
        /// Actualiza un proveedor existente
        /// </summary>
        Task<VendorDto> UpdateVendorAsync(Guid vendorId, Guid organizationId, UpdateVendorDto dto);

        /// <summary>
        /// Elimina un proveedor (solo si no tiene órdenes de compra)
        /// </summary>
        Task DeleteVendorAsync(Guid vendorId, Guid organizationId);
    }
}
