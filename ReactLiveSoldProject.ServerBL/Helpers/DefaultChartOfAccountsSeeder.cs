using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Accounting;

namespace ReactLiveSoldProject.ServerBL.Helpers
{
    public class DefaultChartOfAccountsSeeder
    {
        private readonly LiveSoldDbContext _context;

        public DefaultChartOfAccountsSeeder(LiveSoldDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un catálogo de cuentas básico para una nueva organización
        /// Basado en el plan contable mexicano
        /// </summary>
        public async Task SeedDefaultChartOfAccountsAsync(Guid organizationId)
        {
            var accounts = new List<ChartOfAccount>
            {
                // ===== ACTIVOS (1000-1999) =====
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1000",
                    AccountName = "ACTIVOS",
                    AccountType = AccountType.Asset,
                    Description = "Activos totales de la empresa",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1100",
                    AccountName = "Caja y Bancos",
                    AccountType = AccountType.Asset,
                    SystemAccountType = SystemAccountType.Cash,
                    Description = "Efectivo y cuentas bancarias",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1101",
                    AccountName = "Caja General",
                    AccountType = AccountType.Asset,
                    Description = "Efectivo en caja",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1102",
                    AccountName = "Bancos",
                    AccountType = AccountType.Asset,
                    Description = "Cuentas bancarias",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1200",
                    AccountName = "Cuentas por Cobrar",
                    AccountType = AccountType.Asset,
                    SystemAccountType = SystemAccountType.AccountsReceivable,
                    Description = "Cuentas por cobrar a clientes",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1201",
                    AccountName = "Clientes",
                    AccountType = AccountType.Asset,
                    Description = "Cuentas por cobrar a clientes",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1300",
                    AccountName = "Inventarios",
                    AccountType = AccountType.Asset,
                    SystemAccountType = SystemAccountType.Inventory,
                    Description = "Inventario de mercancías",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "1400",
                    AccountName = "IVA Acreditable",
                    AccountType = AccountType.Asset,
                    Description = "IVA pagado en compras",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ===== PASIVOS (2000-2999) =====
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "2000",
                    AccountName = "PASIVOS",
                    AccountType = AccountType.Liability,
                    Description = "Pasivos totales de la empresa",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "2100",
                    AccountName = "Cuentas por Pagar",
                    AccountType = AccountType.Liability,
                    SystemAccountType = SystemAccountType.AccountsPayable,
                    Description = "Cuentas por pagar a proveedores",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "2101",
                    AccountName = "Proveedores",
                    AccountType = AccountType.Liability,
                    Description = "Deudas con proveedores",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "2200",
                    AccountName = "IVA por Pagar",
                    AccountType = AccountType.Liability,
                    Description = "IVA cobrado en ventas",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "2300",
                    AccountName = "Impuestos por Pagar",
                    AccountType = AccountType.Liability,
                    Description = "Otros impuestos por pagar",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ===== CAPITAL (3000-3999) =====
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "3000",
                    AccountName = "CAPITAL",
                    AccountType = AccountType.Equity,
                    Description = "Capital contable",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "3100",
                    AccountName = "Capital Social",
                    AccountType = AccountType.Equity,
                    Description = "Capital aportado por socios",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "3200",
                    AccountName = "Utilidades Retenidas",
                    AccountType = AccountType.Equity,
                    Description = "Utilidades de ejercicios anteriores",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "3300",
                    AccountName = "Utilidad del Ejercicio",
                    AccountType = AccountType.Equity,
                    Description = "Utilidad o pérdida del ejercicio actual",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ===== INGRESOS (4000-4999) =====
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "4000",
                    AccountName = "INGRESOS",
                    AccountType = AccountType.Revenue,
                    Description = "Ingresos totales",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "4100",
                    AccountName = "Ventas",
                    AccountType = AccountType.Revenue,
                    SystemAccountType = SystemAccountType.SalesRevenue,
                    Description = "Ingresos por ventas",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "4200",
                    AccountName = "Otros Ingresos",
                    AccountType = AccountType.Revenue,
                    Description = "Ingresos diversos",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ===== COSTOS (5000-5999) =====
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "5000",
                    AccountName = "COSTO DE VENTAS",
                    AccountType = AccountType.Expense,
                    Description = "Costos de ventas",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "5100",
                    AccountName = "Costo de Ventas",
                    AccountType = AccountType.Expense,
                    SystemAccountType = SystemAccountType.CostOfGoodsSold,
                    Description = "Costo de mercancías vendidas",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },

                // ===== GASTOS (6000-6999) =====
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6000",
                    AccountName = "GASTOS DE OPERACIÓN",
                    AccountType = AccountType.Expense,
                    Description = "Gastos operativos",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6100",
                    AccountName = "Gastos de Administración",
                    AccountType = AccountType.Expense,
                    Description = "Gastos administrativos",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6101",
                    AccountName = "Sueldos y Salarios",
                    AccountType = AccountType.Expense,
                    Description = "Nómina de empleados",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6102",
                    AccountName = "Renta",
                    AccountType = AccountType.Expense,
                    Description = "Renta de oficinas y locales",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6103",
                    AccountName = "Servicios Públicos",
                    AccountType = AccountType.Expense,
                    Description = "Luz, agua, teléfono, internet",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6200",
                    AccountName = "Gastos de Venta",
                    AccountType = AccountType.Expense,
                    Description = "Gastos relacionados con ventas",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6201",
                    AccountName = "Publicidad y Marketing",
                    AccountType = AccountType.Expense,
                    Description = "Gastos de publicidad",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new ChartOfAccount
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    AccountCode = "6202",
                    AccountName = "Comisiones sobre Ventas",
                    AccountType = AccountType.Expense,
                    Description = "Comisiones a vendedores",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.ChartOfAccounts.AddRangeAsync(accounts);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Crea la configuración de cuentas por defecto para la organización
        /// </summary>
        public async Task CreateDefaultAccountConfigurationAsync(Guid organizationId)
        {
            // Buscar las cuentas que acabamos de crear
            var inventoryAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.Inventory);

            var accountsPayableAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.AccountsPayable);

            var accountsReceivableAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.AccountsReceivable);

            var salesRevenueAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.SalesRevenue);

            var costOfGoodsSoldAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.CostOfGoodsSold);

            var cashAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.Cash);

            var taxPayableAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.AccountCode == "2200"); // IVA por Pagar

            var taxReceivableAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.AccountCode == "1400"); // IVA Acreditable

            var bankAccount = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.AccountCode == "1102"); // Bancos

            // Crear configuración
            var config = new OrganizationAccountConfiguration
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                InventoryAccountId = inventoryAccount?.Id,
                AccountsPayableAccountId = accountsPayableAccount?.Id,
                AccountsReceivableAccountId = accountsReceivableAccount?.Id,
                SalesRevenueAccountId = salesRevenueAccount?.Id,
                CostOfGoodsSoldAccountId = costOfGoodsSoldAccount?.Id,
                TaxPayableAccountId = taxPayableAccount?.Id,
                TaxReceivableAccountId = taxReceivableAccount?.Id,
                CashAccountId = cashAccount?.Id,
                DefaultBankAccountId = bankAccount?.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _context.OrganizationAccountConfigurations.AddAsync(config);
            await _context.SaveChangesAsync();
        }
    }
}
