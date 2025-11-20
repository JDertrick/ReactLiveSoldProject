using System.ComponentModel.DataAnnotations;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;

namespace ReactLiveSoldProject.ServerBL.Models.Sales
{
    public class SalesOrder
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El ID de la organización es obligatorio")]
        public Guid OrganizationId { get; set; }

        public virtual Organization Organization { get; set; }

        [Required(ErrorMessage = "El ID del cliente es obligatorio")]
        public Guid CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public virtual User? CreatedByUser { get; set; }

        [Required(ErrorMessage = "El estado de la orden es obligatorio")]
        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        [Required(ErrorMessage = "El monto total es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto total debe ser mayor o igual a 0")]
        public decimal TotalAmount { get; set; } = 0.00m;

        // ==================== CAMPOS DE IMPUESTOS ====================

        /// <summary>
        /// Subtotal de la orden (suma de todos los items sin impuestos)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor o igual a 0")]
        public decimal SubtotalAmount { get; set; } = 0.00m;

        /// <summary>
        /// Total de impuestos de la orden (suma de impuestos de todos los items)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El total de impuestos debe ser mayor o igual a 0")]
        public decimal TotalTaxAmount { get; set; } = 0.00m;

        [MaxLength(2000, ErrorMessage = "Las notas no pueden exceder los 2000 caracteres")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public virtual ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
        
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
