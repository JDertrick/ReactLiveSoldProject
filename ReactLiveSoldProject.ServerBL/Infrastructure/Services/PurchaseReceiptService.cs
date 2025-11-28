using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Purchases;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Vendors;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class PurchaseReceiptService : IPurchaseReceiptService
    {
        private readonly LiveSoldDbContext _context;
        private readonly IMapper _mapper;

        public PurchaseReceiptService(LiveSoldDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region CRUD Básico

        /// <summary>
        /// Obtiene todas las recepciones de compra con filtros opcionales
        /// </summary>
        public async Task<List<PurchaseReceiptDto>> GetPurchaseReceiptsAsync(
            Guid organizationId,
            string? searchTerm = null,
            string? status = null)
        {
            var query = _context.Set<PurchaseReceipt>()
                .Include(pr => pr.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(pr => pr.ReceivedByUser)
                .Include(pr => pr.WarehouseLocation)
                .Include(pr => pr.PurchaseOrder)
                .Where(pr => pr.OrganizationId == organizationId);

            // Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(pr =>
                    pr.ReceiptNumber.ToLower().Contains(lowerSearch) ||
                    (pr.Vendor.Contact != null &&
                     (pr.Vendor.Contact.FirstName != null && pr.Vendor.Contact.FirstName.ToLower().Contains(lowerSearch)) ||
                     (pr.Vendor.Contact.LastName != null && pr.Vendor.Contact.LastName.ToLower().Contains(lowerSearch)) ||
                     (pr.Vendor.Contact.Company != null && pr.Vendor.Contact.Company.ToLower().Contains(lowerSearch)))
                );
            }

            // Filtro de estado
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseReceiptStatus>(status, true, out var statusEnum))
            {
                query = query.Where(pr => pr.Status == statusEnum);
            }

            var receipts = await query
                .OrderByDescending(pr => pr.ReceiptDate)
                .ToListAsync();

            return receipts.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Obtiene una recepción por ID con todos sus items
        /// </summary>
        public async Task<PurchaseReceiptDto?> GetPurchaseReceiptByIdAsync(Guid receiptId, Guid organizationId)
        {
            var receipt = await _context.Set<PurchaseReceipt>()
                .Include(pr => pr.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(pr => pr.ReceivedByUser)
                .Include(pr => pr.WarehouseLocation)
                .Include(pr => pr.PurchaseOrder)
                .Include(pr => pr.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .Include(pr => pr.PurchaseItems)
                    .ThenInclude(pi => pi.ProductVariant)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId && pr.OrganizationId == organizationId);

            return receipt != null ? MapToDto(receipt) : null;
        }

        /// <summary>
        /// Crea una nueva recepción de compra (sin recibir aún)
        /// </summary>
        public async Task<PurchaseReceiptDto> CreatePurchaseReceiptAsync(
            Guid organizationId,
            Guid userId,
            CreatePurchaseReceiptDto dto)
        {
            try
            {
                // Validar que el proveedor exista y pertenezca a la organización
                var vendor = await _context.Set<Vendor>()
                    .FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.OrganizationId == organizationId);

                if (vendor == null)
                    throw new InvalidOperationException("El proveedor no existe o no pertenece a esta organización");

                // Generar número de recepción
                var receiptNumber = await GenerateReceiptNumberAsync(organizationId);

                // Crear recepción
                var receipt = new PurchaseReceipt
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    ReceiptNumber = receiptNumber,
                    PurchaseOrderId = dto.PurchaseOrderId,
                    VendorId = dto.VendorId,
                    ReceiptDate = DateTime.SpecifyKind(dto.ReceiptDate, DateTimeKind.Utc),
                    Status = PurchaseReceiptStatus.Pending,
                    WarehouseLocationId = dto.WarehouseLocationId,
                    ReceivedBy = userId,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Set<PurchaseReceipt>().Add(receipt);

                // Crear items de recepción
                foreach (var itemDto in dto.Items)
                {
                    var item = new PurchaseItem
                    {
                        Id = Guid.NewGuid(),
                        LineNumber = itemDto.LineNumber,
                        PurchaseReceiptId = receipt.Id,
                        ProductId = itemDto.ProductId,
                        ProductVariantId = itemDto.ProductVariantId,
                        Description = itemDto.Description,
                        QuantityReceived = itemDto.QuantityReceived,
                        UnitCost = itemDto.UnitCost,
                        DiscountPercentage = itemDto.DiscountPercentage,
                        TaxRate = itemDto.TaxRate,
                        GLInventoryAccountId = itemDto.GLInventoryAccountId,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Calcular montos
                    CalculateItemAmounts(item);

                    _context.Set<PurchaseItem>().Add(item);
                }

                await _context.SaveChangesAsync();

                return await GetPurchaseReceiptByIdAsync(receipt.Id, organizationId)
                    ?? throw new InvalidOperationException("Error al crear la recepción");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Elimina una recepción (solo si está en estado Pending)
        /// </summary>
        public async Task DeletePurchaseReceiptAsync(Guid receiptId, Guid organizationId)
        {
            var receipt = await _context.Set<PurchaseReceipt>()
                .Include(pr => pr.PurchaseItems)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId && pr.OrganizationId == organizationId);

            if (receipt == null)
                throw new KeyNotFoundException("Recepción no encontrada");

            if (receipt.Status != PurchaseReceiptStatus.Pending)
                throw new InvalidOperationException("Solo se pueden eliminar recepciones en estado Pending");

            _context.Set<PurchaseItem>().RemoveRange(receipt.PurchaseItems);
            _context.Set<PurchaseReceipt>().Remove(receipt);

            await _context.SaveChangesAsync();
        }

        #endregion

        #region LÓGICA CRÍTICA: Recibir Mercancía

        /// <summary>
        /// ⚡ MÉTODO CRÍTICO: Recibe la mercancía y ejecuta toda la lógica automática
        /// 1. Actualiza estado de la recepción a Received
        /// 2. Genera StockMovement para cada item
        /// 3. Crea StockBatch para FIFO costing
        /// 4. Actualiza stock del producto
        /// 5. Genera JournalEntry automático (Inventario DEBE / CxP HABER)
        /// </summary>
        public async Task<PurchaseReceiptDto> ReceivePurchaseAsync(
            Guid organizationId,
            Guid userId,
            ReceivePurchaseDto dto)
        {
            try
            {
                // 1. Obtener la recepción con todos sus datos
                var receipt = await GetReceiptForProcessing(dto.PurchaseReceiptId, organizationId);

                // 2. Validar que se puede recibir
                ValidateReceiptCanBeReceived(receipt);

                // 3. Procesar cada item: StockMovement + StockBatch + Actualizar inventario
                await ProcessReceiptItems(receipt, userId);

                // 4. Generar asiento contable automático
                var journalEntry = await GenerateReceivingJournalEntry(
                    receipt,
                    organizationId,
                    userId,
                    dto.DefaultGLInventoryAccountId,
                    dto.DefaultGLAccountsPayableId,
                    dto.DefaultGLTaxAccountId);

                // 5. Actualizar estado de la recepción
                receipt.Status = PurchaseReceiptStatus.Received;
                receipt.ReceivingJournalEntryId = journalEntry.Id;
                receipt.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return await GetPurchaseReceiptByIdAsync(receipt.Id, organizationId)
                    ?? throw new InvalidOperationException("Error al procesar la recepción");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Métodos Privados de Soporte

        /// <summary>
        /// Obtiene la recepción con todos los datos necesarios para procesarla
        /// </summary>
        private async Task<PurchaseReceipt> GetReceiptForProcessing(Guid receiptId, Guid organizationId)
        {
            var receipt = await _context.Set<PurchaseReceipt>()
                .Include(pr => pr.Vendor)
                .Include(pr => pr.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .Include(pr => pr.PurchaseItems)
                    .ThenInclude(pi => pi.ProductVariant)
                .FirstOrDefaultAsync(pr => pr.Id == receiptId && pr.OrganizationId == organizationId);

            if (receipt == null)
                throw new KeyNotFoundException("Recepción no encontrada");

            return receipt;
        }

        /// <summary>
        /// Valida que la recepción pueda ser procesada
        /// </summary>
        private void ValidateReceiptCanBeReceived(PurchaseReceipt receipt)
        {
            if (receipt.Status == PurchaseReceiptStatus.Received)
                throw new InvalidOperationException("Esta recepción ya ha sido procesada");

            if (receipt.PurchaseItems == null || !receipt.PurchaseItems.Any())
                throw new InvalidOperationException("La recepción no tiene items");
        }

        /// <summary>
        /// Procesa cada item de la recepción: crea movimientos de stock, batches FIFO y actualiza inventario
        /// </summary>
        private async Task ProcessReceiptItems(PurchaseReceipt receipt, Guid userId)
        {
            foreach (var item in receipt.PurchaseItems)
            {
                // 1. Crear movimiento de inventario (entrada)
                await CreateStockMovement(receipt, item, userId);

                // 2. Crear batch FIFO
                await CreateStockBatch(receipt, item);

                // 3. Actualizar stock del producto/variante
                await UpdateProductStock(item.ProductId, item.ProductVariantId, item.QuantityReceived);
            }
        }

        /// <summary>
        /// Crea un movimiento de stock por cada item recibido
        /// </summary>
        private async Task CreateStockMovement(PurchaseReceipt receipt, PurchaseItem item, Guid userId)
        {
            // Obtener el ProductVariantId correcto
            Guid productVariantId;
            if (item.ProductVariantId.HasValue)
            {
                productVariantId = item.ProductVariantId.Value;
            }
            else
            {
                // Para productos sin variante especificada, buscar la variante por defecto
                // Primero intentar buscar la variante primaria
                var defaultVariant = await _context.Set<ProductVariant>()
                    .FirstOrDefaultAsync(pv => pv.ProductId == item.ProductId && pv.IsPrimary);

                // Si no hay variante primaria, buscar la primera variante disponible
                if (defaultVariant == null)
                {
                    defaultVariant = await _context.Set<ProductVariant>()
                        .FirstOrDefaultAsync(pv => pv.ProductId == item.ProductId);
                }

                if (defaultVariant == null)
                {
                    throw new InvalidOperationException(
                        $"No se encontró ninguna variante para el producto {item.Product?.Name ?? item.ProductId.ToString()}. " +
                        "El producto debe tener al menos una variante.");
                }

                productVariantId = defaultVariant.Id;
            }

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                OrganizationId = receipt.OrganizationId,
                ProductVariantId = productVariantId,
                MovementType = StockMovementType.Purchase,
                Quantity = item.QuantityReceived,
                StockBefore = 0, // Se actualizará con el stock actual antes del movimiento
                StockAfter = 0, // Se actualizará con el stock actual después del movimiento
                UnitCost = item.UnitCost,
                DestinationLocationId = receipt.WarehouseLocationId,
                Reference = receipt.ReceiptNumber,
                Notes = $"Recepción de compra: {receipt.ReceiptNumber}",
                CreatedByUserId = userId,
                IsPosted = true,
                PostedAt = DateTime.UtcNow,
                PostedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<StockMovement>().Add(movement);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Crea un batch FIFO para control de costos
        /// </summary>
        private async Task CreateStockBatch(PurchaseReceipt receipt, PurchaseItem item)
        {
            // Obtener el ProductVariantId correcto
            Guid? productVariantId = item.ProductVariantId;

            if (!productVariantId.HasValue)
            {
                // Para productos sin variante especificada, buscar la variante por defecto
                // Primero intentar buscar la variante primaria
                var defaultVariant = await _context.Set<ProductVariant>()
                    .FirstOrDefaultAsync(pv => pv.ProductId == item.ProductId && pv.IsPrimary);

                // Si no hay variante primaria, buscar la primera variante disponible
                if (defaultVariant == null)
                {
                    defaultVariant = await _context.Set<ProductVariant>()
                        .FirstOrDefaultAsync(pv => pv.ProductId == item.ProductId);
                }

                if (defaultVariant != null)
                {
                    productVariantId = defaultVariant.Id;
                }
            }

            var batch = new StockBatch
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductVariantId = productVariantId,
                PurchaseReceiptId = receipt.Id,
                QuantityRemaining = item.QuantityReceived,
                UnitCost = item.UnitCost,
                ReceiptDate = receipt.ReceiptDate,
                LocationId = receipt.WarehouseLocationId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Set<StockBatch>().Add(batch);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Actualiza el stock actual del producto o variante
        /// </summary>
        private async Task UpdateProductStock(Guid productId, Guid? variantId, int quantity)
        {
            ProductVariant? variant = null;

            if (variantId.HasValue)
            {
                // Actualizar stock de la variante específica
                variant = await _context.Set<ProductVariant>()
                    .FirstOrDefaultAsync(pv => pv.Id == variantId.Value);
            }
            else
            {
                // Para productos sin variante especificada, buscar la variante por defecto
                // Primero intentar buscar la variante primaria
                variant = await _context.Set<ProductVariant>()
                    .FirstOrDefaultAsync(pv => pv.ProductId == productId && pv.IsPrimary);

                // Si no hay variante primaria, buscar la primera variante disponible
                if (variant == null)
                {
                    variant = await _context.Set<ProductVariant>()
                        .FirstOrDefaultAsync(pv => pv.ProductId == productId);
                }
            }

            if (variant != null)
            {
                variant.StockQuantity += quantity;
                variant.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException(
                    $"No se encontró ninguna variante del producto para actualizar el stock. ProductId: {productId}, VariantId: {variantId}");
            }
        }

        /// <summary>
        /// ⚡ GENERA EL ASIENTO CONTABLE AUTOMÁTICO
        /// DEBE: Inventario + IVA Acreditable
        /// HABER: Cuentas por Pagar
        /// </summary>
        private async Task<JournalEntry> GenerateReceivingJournalEntry(
            PurchaseReceipt receipt,
            Guid organizationId,
            Guid userId,
            Guid? defaultInventoryAccountId,
            Guid? defaultPayableAccountId,
            Guid? defaultTaxAccountId)
        {
            try
            {
                // Calcular totales
                var subtotal = receipt.PurchaseItems.Sum(i => i.QuantityReceived * i.UnitCost * (1 - i.DiscountPercentage / 100));
                var taxAmount = receipt.PurchaseItems.Sum(i => i.TaxAmount);
                var total = subtotal + taxAmount;

                // Generar el número de asiento contable
                var entryNumber = await GenerateJournalEntryNumberAsync(organizationId);

                // Crear el Journal Entry
                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    EntryNumber = entryNumber,
                    EntryDate = receipt.ReceiptDate,
                    Description = $"Recepción de compra {receipt.ReceiptNumber} - Proveedor: {receipt.Vendor?.Contact?.Company ?? "N/A"}",
                    ReferenceNumber = receipt.ReceiptNumber,
                    DocumentType = "RECEIPT",
                    DocumentNumber = receipt.ReceiptNumber,
                    VendorId = receipt.VendorId,
                    PostedBy = userId,
                    PostedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    Currency = "MXN",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<JournalEntry>().Add(journalEntry);

                // Obtener las cuentas contables
                var inventoryAccount = await GetOrCreateInventoryAccount(organizationId, defaultInventoryAccountId);
                var payableAccount = await GetOrCreatePayableAccount(organizationId, defaultPayableAccountId);
                var taxAccount = taxAmount > 0 ? await GetOrCreateTaxAccount(organizationId, defaultTaxAccountId) : null;

                // Línea 1: DEBE - Inventario
                var line1 = new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = inventoryAccount.Id,
                    DebitAmount = subtotal,
                    CreditAmount = 0,
                    Description = $"Inventario recibido - {receipt.ReceiptNumber}"
                };
                _context.Set<JournalEntryLine>().Add(line1);

                // Línea 2: DEBE - IVA Acreditable (si aplica)
                if (taxAmount > 0 && taxAccount != null)
                {
                    var line2 = new JournalEntryLine
                    {
                        Id = Guid.NewGuid(),
                        JournalEntryId = journalEntry.Id,
                        AccountId = taxAccount.Id,
                        DebitAmount = taxAmount,
                        CreditAmount = 0,
                        Description = "IVA Acreditable"
                    };
                    _context.Set<JournalEntryLine>().Add(line2);
                }

                // Línea 3: HABER - Cuentas por Pagar
                var line3 = new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = payableAccount.Id,
                    DebitAmount = 0,
                    CreditAmount = total,
                    Description = $"CxP Proveedor: {receipt.Vendor?.Contact?.Company ?? "N/A"}"
                };
                _context.Set<JournalEntryLine>().Add(line3);

                return journalEntry;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Obtiene o crea la cuenta de Inventario
        /// </summary>
        private async Task<ChartOfAccount> GetOrCreateInventoryAccount(Guid organizationId, Guid? accountId)
        {
            if (accountId.HasValue)
            {
                var account = await _context.Set<ChartOfAccount>()
                    .FirstOrDefaultAsync(c => c.Id == accountId.Value && c.OrganizationId == organizationId);
                if (account != null) return account;
            }

            // Buscar en la configuración de la organización
            var config = await _context.OrganizationAccountConfigurations
                .Include(c => c.InventoryAccount)
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId);

            if (config?.InventoryAccount != null)
                return config.InventoryAccount;

            // Buscar cuenta por SystemAccountType como fallback
            var inventoryAccount = await _context.Set<ChartOfAccount>()
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.Inventory);

            if (inventoryAccount == null)
                throw new InvalidOperationException("No se encontró la cuenta contable de Inventario. " +
                    "Por favor, configure las cuentas contables en 'Configuración Contable'.");

            return inventoryAccount;
        }

        /// <summary>
        /// Obtiene o crea la cuenta de Cuentas por Pagar
        /// </summary>
        private async Task<ChartOfAccount> GetOrCreatePayableAccount(Guid organizationId, Guid? accountId)
        {
            if (accountId.HasValue)
            {
                var account = await _context.Set<ChartOfAccount>()
                    .FirstOrDefaultAsync(c => c.Id == accountId.Value && c.OrganizationId == organizationId);
                if (account != null) return account;
            }

            // Buscar en la configuración de la organización
            var config = await _context.OrganizationAccountConfigurations
                .Include(c => c.AccountsPayableAccount)
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId);

            if (config?.AccountsPayableAccount != null)
                return config.AccountsPayableAccount;

            // Buscar cuenta por SystemAccountType como fallback
            var payableAccount = await _context.Set<ChartOfAccount>()
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         c.SystemAccountType == SystemAccountType.AccountsPayable);

            if (payableAccount == null)
                throw new InvalidOperationException("No se encontró la cuenta contable de Cuentas por Pagar. " +
                    "Por favor, configure las cuentas contables en 'Configuración Contable'.");

            return payableAccount;
        }

        /// <summary>
        /// Obtiene la cuenta de IVA Acreditable/Tax Receivable
        /// </summary>
        private async Task<ChartOfAccount?> GetOrCreateTaxAccount(Guid organizationId, Guid? accountId)
        {
            if (accountId.HasValue)
            {
                return await _context.Set<ChartOfAccount>()
                    .FirstOrDefaultAsync(c => c.Id == accountId.Value && c.OrganizationId == organizationId);
            }

            // Buscar en la configuración de la organización
            var config = await _context.OrganizationAccountConfigurations
                .Include(c => c.TaxReceivableAccount)
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId);

            if (config?.TaxReceivableAccount != null)
                return config.TaxReceivableAccount;

            // Buscar cuenta de impuestos por código o nombre como fallback
            return await _context.Set<ChartOfAccount>()
                .FirstOrDefaultAsync(c => c.OrganizationId == organizationId &&
                                         (c.AccountCode.Contains("1105") || c.AccountName.Contains("IVA Acreditable")));
        }

        /// <summary>
        /// Genera un número de recepción único
        /// </summary>
        private async Task<string> GenerateReceiptNumberAsync(Guid organizationId)
        {
            var lastReceipt = await _context.Set<PurchaseReceipt>()
                .Where(pr => pr.OrganizationId == organizationId)
                .OrderByDescending(pr => pr.CreatedAt)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastReceipt != null && lastReceipt.ReceiptNumber.StartsWith("RCV-"))
            {
                var numberPart = lastReceipt.ReceiptNumber.Substring(4);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"RCV-{nextNumber:D5}";
        }

        /// <summary>
        /// Genera un número de asiento contable único en formato JE-YYYY-000001
        /// </summary>
        private async Task<string> GenerateJournalEntryNumberAsync(Guid organizationId)
        {
            var currentYear = DateTime.UtcNow.Year;
            var prefix = $"JE-{currentYear}-";

            var lastEntry = await _context.Set<JournalEntry>()
                .Where(je => je.OrganizationId == organizationId && je.EntryNumber.StartsWith(prefix))
                .OrderByDescending(je => je.EntryNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastEntry != null)
            {
                // Extraer el número de la parte final (JE-2025-000001 -> 000001)
                var numberPart = lastEntry.EntryNumber.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D6}";
        }

        /// <summary>
        /// Calcula los montos de un item (TaxAmount, LineTotal)
        /// </summary>
        private void CalculateItemAmounts(PurchaseItem item)
        {
            var subtotal = item.QuantityReceived * item.UnitCost;
            var discountAmount = subtotal * (item.DiscountPercentage / 100);
            var subtotalAfterDiscount = subtotal - discountAmount;

            item.TaxAmount = subtotalAfterDiscount * (item.TaxRate / 100);
            item.LineTotal = subtotalAfterDiscount + item.TaxAmount;
        }

        /// <summary>
        /// Mapea la entidad a DTO
        /// </summary>
        private PurchaseReceiptDto MapToDto(PurchaseReceipt receipt)
        {
            return new PurchaseReceiptDto
            {
                Id = receipt.Id,
                OrganizationId = receipt.OrganizationId,
                ReceiptNumber = receipt.ReceiptNumber,
                PurchaseOrderId = receipt.PurchaseOrderId,
                PurchaseOrderNumber = receipt.PurchaseOrder?.PONumber,
                VendorId = receipt.VendorId,
                VendorName = receipt.Vendor?.Contact?.Company ??
                            $"{receipt.Vendor?.Contact?.FirstName} {receipt.Vendor?.Contact?.LastName}".Trim(),
                ReceiptDate = receipt.ReceiptDate,
                Status = receipt.Status,
                WarehouseLocationId = receipt.WarehouseLocationId,
                WarehouseLocationName = receipt.WarehouseLocation?.Name,
                ReceivedBy = receipt.ReceivedBy,
                ReceivedByName = $"{receipt.ReceivedByUser?.FirstName} {receipt.ReceivedByUser?.LastName}".Trim(),
                Notes = receipt.Notes,
                ReceivingJournalEntryId = receipt.ReceivingJournalEntryId,
                CreatedAt = receipt.CreatedAt,
                UpdatedAt = receipt.UpdatedAt,
                Items = receipt.PurchaseItems?.Select(MapItemToDto).ToList()
            };
        }

        private PurchaseItemDto MapItemToDto(PurchaseItem item)
        {
            return new PurchaseItemDto
            {
                Id = item.Id,
                LineNumber = item.LineNumber,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name,
                ProductVariantId = item.ProductVariantId,
                VariantName = item.ProductVariant != null
                    ? $"{item.ProductVariant.Size ?? ""} {item.ProductVariant.Color ?? ""}".Trim()
                    : null,
                Description = item.Description,
                QuantityOrdered = item.QuantityOrdered,
                QuantityReceived = item.QuantityReceived,
                UnitCost = item.UnitCost,
                DiscountPercentage = item.DiscountPercentage,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                LineTotal = item.LineTotal
            };
        }

        #endregion
    }
}
