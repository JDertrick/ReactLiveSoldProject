using ReactLiveSoldProject.ServerBL.DTOs.Purchases;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IPaymentTermsService
    {
        Task<List<PaymentTermsDto>> GetPaymentTermsAsync(Guid organizationId);
        Task<PaymentTermsDto?> GetPaymentTermsByIdAsync(Guid paymentTermsId, Guid organizationId);
        Task<PaymentTermsDto> CreatePaymentTermsAsync(Guid organizationId, CreatePaymentTermsDto dto);
        Task<PaymentTermsDto> UpdatePaymentTermsAsync(Guid paymentTermsId, Guid organizationId, UpdatePaymentTermsDto dto);
        Task DeletePaymentTermsAsync(Guid paymentTermsId, Guid organizationId);
    }
}
