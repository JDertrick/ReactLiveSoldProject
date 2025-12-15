using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Helpers
{
    /// <summary>
    /// Seed de series numéricas por defecto para una nueva organización
    /// </summary>
    public class DefaultNoSeriesSeeder
    {
        private readonly LiveSoldDbContext _context;

        public DefaultNoSeriesSeeder(LiveSoldDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea series numéricas por defecto para todos los tipos de documentos
        /// </summary>
        public async Task SeedDefaultNoSeriesAsync(Guid organizationId)
        {
            try
            {
                var currentYear = DateTime.UtcNow.Year;
                var series = new List<NoSerie>
                {
                    // CUSTOMER - Serie para Clientes
                    CreateSerie(organizationId, "CUST", "Numeración de Clientes", DocumentType.Customer, true,
                        CreateLine($"CUST-{currentYear}-0001", $"CUST-{currentYear}-9999", $"CUST-{currentYear}-9900")),

                    // VENDOR - Serie para Proveedores
                    CreateSerie(organizationId, "VEND", "Numeración de Proveedores", DocumentType.Vendor, true,
                        CreateLine($"VEND-{currentYear}-0001", $"VEND-{currentYear}-9999", $"VEND-{currentYear}-9900")),

                    // SALES INVOICE - Serie para Facturas de Venta
                    CreateSerie(organizationId, "SINV", "Facturas de Venta", DocumentType.SalesInvoice, true,
                        CreateLine($"SINV-{currentYear}-0001", $"SINV-{currentYear}-9999", $"SINV-{currentYear}-9800")),

                    // SALES ORDER - Serie para Órdenes de Venta
                    CreateSerie(organizationId, "SO", "Órdenes de Venta", DocumentType.SalesOrder, true,
                        CreateLine($"SO-{currentYear}-0001", $"SO-{currentYear}-9999", $"SO-{currentYear}-9800")),

                    // PURCHASE INVOICE - Serie para Facturas de Compra
                    CreateSerie(organizationId, "PINV", "Facturas de Compra", DocumentType.PurchaseInvoice, true,
                        CreateLine($"PINV-{currentYear}-0001", $"PINV-{currentYear}-9999", $"PINV-{currentYear}-9800")),

                    // PURCHASE ORDER - Serie para Órdenes de Compra
                    CreateSerie(organizationId, "PO", "Órdenes de Compra", DocumentType.PurchaseOrder, true,
                        CreateLine($"PO-{currentYear}-0001", $"PO-{currentYear}-9999", $"PO-{currentYear}-9800")),

                    // PURCHASE RECEIPT - Serie para Recepciones de Compra
                    CreateSerie(organizationId, "PREC", "Recepciones de Compra", DocumentType.PurchaseReceipt, true,
                        CreateLine($"PREC-{currentYear}-0001", $"PREC-{currentYear}-9999", $"PREC-{currentYear}-9800")),

                    // PRODUCT - Serie para Productos
                    CreateSerie(organizationId, "PROD", "Numeración de Productos", DocumentType.Product, true,
                        CreateLine("PROD-0001", "PROD-99999", "PROD-99000")),

                    // PRODUCT VARIANT - Serie para Variantes de Producto
                    CreateSerie(organizationId, "VAR", "Variantes de Producto", DocumentType.ProductVariant, true,
                        CreateLine("VAR-0001", "VAR-99999", "VAR-99000")),

                    // PAYMENT - Serie para Pagos
                    CreateSerie(organizationId, "PAY", "Pagos", DocumentType.Payment, true,
                        CreateLine($"PAY-{currentYear}-0001", $"PAY-{currentYear}-9999", $"PAY-{currentYear}-9800")),

                    // JOURNAL ENTRY - Serie para Asientos Contables
                    CreateSerie(organizationId, "JE", "Asientos Contables", DocumentType.JournalEntry, true,
                        CreateLine($"JE-{currentYear}-0001", $"JE-{currentYear}-9999", $"JE-{currentYear}-9800")),

                    // WALLET TRANSACTION - Serie para Transacciones de Billetera
                    CreateSerie(organizationId, "WT", "Transacciones de Billetera", DocumentType.WalletTransaction, true,
                        CreateLine($"WT-{currentYear}-0001", $"WT-{currentYear}-99999", $"WT-{currentYear}-99000")),

                    // STOCK MOVEMENT - Serie para Movimientos de Inventario
                    CreateSerie(organizationId, "SM", "Movimientos de Inventario", DocumentType.StockMovement, true,
                        CreateLine($"SM-{currentYear}-0001", $"SM-{currentYear}-99999", $"SM-{currentYear}-99000")),

                    // CONTACT - Serie para Contactos
                    CreateSerie(organizationId, "CONT", "Numeración de Contactos", DocumentType.Contact, true,
                        CreateLine("CONT-0001", "CONT-99999", "CONT-99000"))
                };

                _context.NoSeries.AddRange(series);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Crea una serie numérica con una línea por defecto
        /// </summary>
        private static NoSerie CreateSerie(
            Guid organizationId,
            string code,
            string description,
            DocumentType documentType,
            bool isDefault,
            NoSerieLine line)
        {
            var serie = new NoSerie
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Code = code,
                Description = description,
                DocumentType = documentType,
                DefaultNos = isDefault,
                ManualNos = false,
                DateOrder = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                NoSerieLines = new List<NoSerieLine> { line }
            };

            return serie;
        }

        /// <summary>
        /// Crea una línea de numeración
        /// </summary>
        private static NoSerieLine CreateLine(string startingNo, string endingNo, string? warningNo = null)
        {
            return new NoSerieLine
            {
                Id = Guid.NewGuid(),
                StartingDate = DateTime.SpecifyKind(new DateTime(DateTime.UtcNow.Year, 1, 1), DateTimeKind.Utc), // 1 de enero del año actual
                StartingNo = startingNo,
                EndingNo = endingNo,
                WarningNo = warningNo,
                IncrementBy = 1,
                Open = true,
                LastNoUsed = null,
                LastDateUsed = null
            };
        }
    }
}
