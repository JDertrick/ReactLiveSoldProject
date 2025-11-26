using ReactLiveSoldProject.ServerBL.DTOs.Payments;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IPaymentService
    {
        Task<List<PaymentDto>> GetPaymentsAsync(Guid organizationId, Guid? vendorId = null, string? searchTerm = null);
        Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId, Guid organizationId);

        /// <summary>
        /// MÉTODO CRÍTICO: Crea un pago y genera automáticamente:
        /// 1. PaymentApplication(s) a facturas
        /// 2. Actualiza AmountPaid y PaymentStatus de VendorInvoice(s)
        /// 3. Actualiza CompanyBankAccount.CurrentBalance
        /// 4. JournalEntry automático (Cuentas por Pagar DEBE / Banco HABER)
        /// </summary>
        Task<PaymentDto> CreatePaymentAsync(Guid organizationId, Guid userId, CreatePaymentDto dto);

        Task<PaymentDto> VoidPaymentAsync(Guid paymentId, Guid organizationId, Guid userId, VoidPaymentDto dto);
    }
}
