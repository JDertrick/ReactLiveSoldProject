using ReactLiveSoldProject.ServerBL.DTOs.Payments;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces
{
    public interface IPaymentService
    {
        Task<List<PaymentDto>> GetPaymentsAsync(Guid organizationId, Guid? vendorId = null, string? searchTerm = null);
        Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId, Guid organizationId);

        /// <summary>
        /// Crea un pago con estado Pending
        /// </summary>
        Task<PaymentDto> CreatePaymentAsync(Guid organizationId, Guid userId, CreatePaymentDto dto);

        /// <summary>
        /// Aprueba un pago (Pending → Approved)
        /// </summary>
        Task<PaymentDto> ApprovePaymentAsync(Guid paymentId, Guid organizationId, Guid userId);

        /// <summary>
        /// Rechaza un pago (Pending → Rejected)
        /// </summary>
        Task<PaymentDto> RejectPaymentAsync(Guid paymentId, Guid organizationId, Guid userId, string reason);

        /// <summary>
        /// MÉTODO CRÍTICO: Contabiliza un pago (Approved → Posted)
        /// 1. Aplica el pago a las facturas
        /// 2. Actualiza AmountPaid y PaymentStatus de VendorInvoice(s)
        /// 3. Actualiza CompanyBankAccount.CurrentBalance
        /// 4. Genera JournalEntry automático (Cuentas por Pagar DEBE / Banco HABER)
        /// </summary>
        Task<PaymentDto> PostPaymentAsync(Guid paymentId, Guid organizationId, Guid userId);

        Task<PaymentDto> VoidPaymentAsync(Guid paymentId, Guid organizationId, Guid userId, VoidPaymentDto dto);
    }
}
