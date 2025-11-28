namespace ReactLiveSoldProject.ServerBL.DTOs.Accounting
{
    public class OrganizationAccountConfigurationDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }

        // Cuentas de Inventario
        public Guid? InventoryAccountId { get; set; }
        public string? InventoryAccountName { get; set; }

        // Cuentas de Cuentas por Pagar
        public Guid? AccountsPayableAccountId { get; set; }
        public string? AccountsPayableAccountName { get; set; }

        // Cuentas de Cuentas por Cobrar
        public Guid? AccountsReceivableAccountId { get; set; }
        public string? AccountsReceivableAccountName { get; set; }

        // Cuentas de Ingresos
        public Guid? SalesRevenueAccountId { get; set; }
        public string? SalesRevenueAccountName { get; set; }

        // Cuentas de Costo de Ventas
        public Guid? CostOfGoodsSoldAccountId { get; set; }
        public string? CostOfGoodsSoldAccountName { get; set; }

        // Cuentas de IVA
        public Guid? TaxPayableAccountId { get; set; }
        public string? TaxPayableAccountName { get; set; }

        public Guid? TaxReceivableAccountId { get; set; }
        public string? TaxReceivableAccountName { get; set; }

        // Cuenta de efectivo/caja
        public Guid? CashAccountId { get; set; }
        public string? CashAccountName { get; set; }

        // Cuenta bancaria por defecto
        public Guid? DefaultBankAccountId { get; set; }
        public string? DefaultBankAccountName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateOrganizationAccountConfigurationDto
    {
        public Guid? InventoryAccountId { get; set; }
        public Guid? AccountsPayableAccountId { get; set; }
        public Guid? AccountsReceivableAccountId { get; set; }
        public Guid? SalesRevenueAccountId { get; set; }
        public Guid? CostOfGoodsSoldAccountId { get; set; }
        public Guid? TaxPayableAccountId { get; set; }
        public Guid? TaxReceivableAccountId { get; set; }
        public Guid? CashAccountId { get; set; }
        public Guid? DefaultBankAccountId { get; set; }
    }
}
