using ReactLiveSoldProject.ServerBL.DTOs.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IVendorInvoiceService
    {
        Task<List<VendorInvoiceDto>> GetVendorInvoicesAsync(Guid organizationId, Guid? vendorId = null, string? status = null);
        Task<VendorInvoiceDto?> GetVendorInvoiceByIdAsync(Guid invoiceId, Guid organizationId);
        Task<VendorInvoiceDto> CreateVendorInvoiceAsync(Guid organizationId, Guid userId, CreateVendorInvoiceDto dto);
        Task<VendorInvoiceDto> UpdateVendorInvoiceAsync(Guid invoiceId, Guid organizationId, CreateVendorInvoiceDto dto);
        Task DeleteVendorInvoiceAsync(Guid invoiceId, Guid organizationId);
    }
}
