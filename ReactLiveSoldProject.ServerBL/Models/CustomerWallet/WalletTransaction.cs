using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Models.CustomerWallet
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID de la billetera es obligatorio")]
        public Guid WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        [Required(ErrorMessage = "El tipo de transacción es obligatorio")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        public Guid? RelatedSalesOrderId { get; set; }

        public virtual SalesOrder? RelatedSalesOrder { get; set; }

        public Guid? AuthorizedByUserId { get; set; }

        public virtual User? AuthorizedByUser { get; set; }

        [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder los 1000 caracteres")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
