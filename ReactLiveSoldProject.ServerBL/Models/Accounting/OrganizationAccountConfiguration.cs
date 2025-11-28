using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReactLiveSoldProject.ServerBL.Base;

namespace ReactLiveSoldProject.ServerBL.Models.Accounting
{
    /// <summary>
    /// Configuración de cuentas contables por defecto para cada organización.
    /// Permite a cada cliente configurar qué cuentas usar para transacciones automáticas.
    /// </summary>
    public class OrganizationAccountConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }

        // Cuentas de Inventario
        public Guid? InventoryAccountId { get; set; }
        public virtual ChartOfAccount? InventoryAccount { get; set; }

        // Cuentas de Cuentas por Pagar
        public Guid? AccountsPayableAccountId { get; set; }
        public virtual ChartOfAccount? AccountsPayableAccount { get; set; }

        // Cuentas de Cuentas por Cobrar
        public Guid? AccountsReceivableAccountId { get; set; }
        public virtual ChartOfAccount? AccountsReceivableAccount { get; set; }

        // Cuentas de Ingresos
        public Guid? SalesRevenueAccountId { get; set; }
        public virtual ChartOfAccount? SalesRevenueAccount { get; set; }

        // Cuentas de Costo de Ventas
        public Guid? CostOfGoodsSoldAccountId { get; set; }
        public virtual ChartOfAccount? CostOfGoodsSoldAccount { get; set; }

        // Cuentas de IVA
        public Guid? TaxPayableAccountId { get; set; } // IVA por pagar (generado en ventas)
        public virtual ChartOfAccount? TaxPayableAccount { get; set; }

        public Guid? TaxReceivableAccountId { get; set; } // IVA acreditable (pagado en compras)
        public virtual ChartOfAccount? TaxReceivableAccount { get; set; }

        // Cuenta de efectivo/caja
        public Guid? CashAccountId { get; set; }
        public virtual ChartOfAccount? CashAccount { get; set; }

        // Cuenta bancaria por defecto
        public Guid? DefaultBankAccountId { get; set; }
        public virtual ChartOfAccount? DefaultBankAccount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
