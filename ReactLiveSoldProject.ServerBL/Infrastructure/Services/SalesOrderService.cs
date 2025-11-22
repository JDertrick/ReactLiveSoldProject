using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IStockMovementService _stockMovementService;
        private readonly ITaxService _taxService;

        public SalesOrderService(LiveSoldDbContext dbContext, IStockMovementService stockMovementService, ITaxService taxService)
        {
            _dbContext = dbContext;
            _stockMovementService = stockMovementService;
            _taxService = taxService;
        }

        public async Task<List<SalesOrderDto>> GetSalesOrdersByOrganizationAsync(Guid organizationId, string? status = null)
        {
            var query = _dbContext.SalesOrders
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Contact)
                .Include(so => so.CreatedByUser)
                .Include(so => so.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Where(so => so.OrganizationId == organizationId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                    query = query.Where(so => so.Status == orderStatus);
            }

            var orders = await query
                .OrderByDescending(so => so.CreatedAt)
                .ToListAsync();

            return orders.Select(o => MapToDto(o)).ToList();
        }

        public async Task<SalesOrderDto?> GetSalesOrderByIdAsync(Guid salesOrderId, Guid organizationId)
        {
            var order = await _dbContext.SalesOrders
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Contact)
                .Include(so => so.CreatedByUser)
                .Include(so => so.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.OrganizationId == organizationId);

            return order != null ? MapToDto(order) : null;
        }

        public async Task<List<SalesOrderDto>> GetSalesOrdersByCustomerIdAsync(Guid customerId, Guid organizationId)
        {
            var orders = await _dbContext.SalesOrders
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Contact)
                .Include(so => so.CreatedByUser)
                .Include(so => so.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Where(so => so.CustomerId == customerId && so.OrganizationId == organizationId)
                .OrderByDescending(so => so.CreatedAt)
                .ToListAsync();

            return orders.Select(o => MapToDto(o)).ToList();
        }

        public async Task<SalesOrderDto> CreateDraftOrderAsync(Guid organizationId, Guid createdByUserId, CreateSalesOrderDto dto)
        {
            // Verificar que el cliente existe y pertenece a la organización
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.OrganizationId == organizationId);

            if (customer == null)
                throw new KeyNotFoundException("Cliente no encontrado");

            // Verificar que el usuario pertenece a la organización
            var userBelongsToOrg = await _dbContext.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == createdByUserId);

            if (!userBelongsToOrg)
                throw new UnauthorizedAccessException("El usuario no pertenece a esta organización");

            // Validar que haya al menos un item
            if (!dto.Items.Any())
                throw new InvalidOperationException("La orden debe tener al menos un item");

            // Crear la orden
            var orderId = Guid.NewGuid();
            var order = new SalesOrder
            {
                Id = orderId,
                OrganizationId = organizationId,
                CustomerId = dto.CustomerId,
                CreatedByUserId = createdByUserId,
                Status = OrderStatus.Draft,
                TotalAmount = 0m,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.SalesOrders.Add(order);

            // Agregar items y calcular el total
            decimal totalAmount = 0m;
            decimal totalSubtotal = 0m;
            decimal totalTax = 0m;

            foreach (var itemDto in dto.Items)
            {
                var variant = await _dbContext.ProductVariants
                    .Include(pv => pv.Product)
                    .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.OrganizationId == organizationId);

                if (variant == null)
                    throw new KeyNotFoundException($"Variante de producto no encontrada: {itemDto.ProductVariantId}");

                // Determinar precio según el tipo de venta
                var defaultPrice = itemDto.SaleType == Base.SaleType.Wholesale
                    ? (variant.WholesalePrice ?? variant.Price)  // Si no tiene precio mayorista, usa detal
                    : variant.Price;  // Precio detal por defecto

                var unitPrice = itemDto.CustomUnitPrice ?? defaultPrice;
                var lineTotal = unitPrice * itemDto.Quantity;

                // Calcular impuestos para este item (si el producto no está exento)
                decimal itemSubtotal = lineTotal;
                decimal itemTaxAmount = 0m;
                decimal itemTotal = lineTotal;
                decimal itemTaxRate = 0m;
                Guid? itemTaxRateId = null;

                if (!variant.Product.IsTaxExempt)
                {
                    var taxCalc = await _taxService.CalculateTaxAsync(organizationId, lineTotal);
                    itemSubtotal = taxCalc.Subtotal;
                    itemTaxAmount = taxCalc.TaxAmount;
                    itemTotal = taxCalc.Total;
                    itemTaxRate = taxCalc.TaxRate;
                    itemTaxRateId = taxCalc.TaxRateId;
                }

                var orderItem = new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    SalesOrderId = orderId,
                    ProductVariantId = itemDto.ProductVariantId,
                    Quantity = itemDto.Quantity,
                    OriginalPrice = variant.Price,
                    UnitPrice = unitPrice,
                    ItemDescription = itemDto.ItemDescription,
                    TaxRateId = itemTaxRateId,
                    TaxRate = itemTaxRate,
                    TaxAmount = itemTaxAmount,
                    Subtotal = itemSubtotal,
                    Total = itemTotal
                };

                _dbContext.SalesOrderItems.Add(orderItem);
                totalSubtotal += itemSubtotal;
                totalTax += itemTaxAmount;
                totalAmount += itemTotal;
            }

            order.TotalAmount = totalAmount;
            order.SubtotalAmount = totalSubtotal;
            order.TotalTaxAmount = totalTax;

            await _dbContext.SaveChangesAsync();

            // Recargar la orden con sus relaciones
            var createdOrder = await _dbContext.SalesOrders
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Contact)
                .Include(so => so.CreatedByUser)
                .Include(so => so.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstAsync(so => so.Id == orderId);

            return MapToDto(createdOrder);
        }

        public async Task<SalesOrderDto> AddItemToOrderAsync(Guid salesOrderId, Guid organizationId, CreateSalesOrderItemDto dto)
        {
            var order = await _dbContext.SalesOrders
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.OrganizationId == organizationId);

            if (order == null)
                throw new KeyNotFoundException("Orden no encontrada");

            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Solo se pueden modificar órdenes en estado Draft");

            var variant = await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == dto.ProductVariantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante de producto no encontrada");

            // Determinar precio según el tipo de venta
            var defaultPrice = dto.SaleType == Base.SaleType.Wholesale
                ? (variant.WholesalePrice ?? variant.Price)
                : variant.Price;

            var unitPrice = dto.CustomUnitPrice ?? defaultPrice;
            var lineTotal = unitPrice * dto.Quantity;

            // Calcular impuestos para este item (si el producto no está exento)
            decimal itemSubtotal = lineTotal;
            decimal itemTaxAmount = 0m;
            decimal itemTotal = lineTotal;
            decimal itemTaxRate = 0m;
            Guid? itemTaxRateId = null;

            if (!variant.Product.IsTaxExempt)
            {
                var taxCalc = await _taxService.CalculateTaxAsync(organizationId, lineTotal);
                itemSubtotal = taxCalc.Subtotal;
                itemTaxAmount = taxCalc.TaxAmount;
                itemTotal = taxCalc.Total;
                itemTaxRate = taxCalc.TaxRate;
                itemTaxRateId = taxCalc.TaxRateId;
            }

            var orderItem = new SalesOrderItem
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                SalesOrderId = salesOrderId,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity,
                OriginalPrice = variant.Price,
                UnitPrice = unitPrice,
                ItemDescription = dto.ItemDescription,
                TaxRateId = itemTaxRateId,
                TaxRate = itemTaxRate,
                TaxAmount = itemTaxAmount,
                Subtotal = itemSubtotal,
                Total = itemTotal
            };

            _dbContext.SalesOrderItems.Add(orderItem);

            // Recalcular totales
            order.SubtotalAmount += itemSubtotal;
            order.TotalTaxAmount += itemTaxAmount;
            order.TotalAmount += itemTotal;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(order).Collection(o => o.Items).LoadAsync();
            foreach (var item in order.Items)
            {
                await _dbContext.Entry(item).Reference(i => i.ProductVariant).LoadAsync();
                await _dbContext.Entry(item.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            }
            await _dbContext.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _dbContext.Entry(order.Customer).Reference(c => c.Contact).LoadAsync();
            await _dbContext.Entry(order).Reference(o => o.CreatedByUser).LoadAsync();

            return MapToDto(order);
        }

        public async Task<SalesOrderDto> UpdateItemInOrderAsync(Guid salesOrderId, Guid itemId, Guid organizationId, UpdateSalesOrderItemDto dto)
        {
            var order = await _dbContext.SalesOrders
                .Include(so => so.Items)
                    .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.OrganizationId == organizationId);

            if (order == null)
                throw new KeyNotFoundException("Orden no encontrada");

            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Solo se pueden modificar órdenes en estado Draft");

            var item = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new KeyNotFoundException("Item no encontrado en la orden");

            // Guardar valores antiguos para recalcular totales
            var oldSubtotal = item.Subtotal;
            var oldTaxAmount = item.TaxAmount;
            var oldTotal = item.Total;

            // Actualizar el item
            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.CustomUnitPrice ?? item.ProductVariant.Price;
            item.ItemDescription = dto.ItemDescription;

            // Recalcular impuestos para este item
            var lineTotal = item.UnitPrice * item.Quantity;
            decimal itemSubtotal = lineTotal;
            decimal itemTaxAmount = 0m;
            decimal itemTotal = lineTotal;
            decimal itemTaxRate = 0m;
            Guid? itemTaxRateId = null;

            // Cargar el Product si no está cargado
            if (item.ProductVariant.Product == null)
            {
                await _dbContext.Entry(item.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            }

            if (!item.ProductVariant.Product.IsTaxExempt)
            {
                var taxCalc = await _taxService.CalculateTaxAsync(organizationId, lineTotal);
                itemSubtotal = taxCalc.Subtotal;
                itemTaxAmount = taxCalc.TaxAmount;
                itemTotal = taxCalc.Total;
                itemTaxRate = taxCalc.TaxRate;
                itemTaxRateId = taxCalc.TaxRateId;
            }

            item.TaxRateId = itemTaxRateId;
            item.TaxRate = itemTaxRate;
            item.TaxAmount = itemTaxAmount;
            item.Subtotal = itemSubtotal;
            item.Total = itemTotal;

            // Recalcular totales de la orden
            order.SubtotalAmount = order.SubtotalAmount - oldSubtotal + itemSubtotal;
            order.TotalTaxAmount = order.TotalTaxAmount - oldTaxAmount + itemTaxAmount;
            order.TotalAmount = order.TotalAmount - oldTotal + itemTotal;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(order).Collection(o => o.Items).LoadAsync();
            foreach (var orderItem in order.Items)
            {
                await _dbContext.Entry(orderItem).Reference(i => i.ProductVariant).LoadAsync();
                await _dbContext.Entry(orderItem.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            }
            await _dbContext.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _dbContext.Entry(order.Customer).Reference(c => c.Contact).LoadAsync();
            await _dbContext.Entry(order).Reference(o => o.CreatedByUser).LoadAsync();

            return MapToDto(order);
        }

        public async Task<SalesOrderDto> RemoveItemFromOrderAsync(Guid salesOrderId, Guid itemId, Guid organizationId)
        {
            var order = await _dbContext.SalesOrders
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.OrganizationId == organizationId);

            if (order == null)
                throw new KeyNotFoundException("Orden no encontrada");

            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Solo se pueden modificar órdenes en estado Draft");

            var item = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new KeyNotFoundException("Item no encontrado en la orden");

            if (order.Items.Count <= 1)
                throw new InvalidOperationException("No se puede eliminar el último item de la orden. Cancele la orden en su lugar.");

            // Guardar valores para ajustar totales
            var itemSubtotal = item.Subtotal;
            var itemTaxAmount = item.TaxAmount;
            var itemTotal = item.Total;

            _dbContext.SalesOrderItems.Remove(item);

            // Recalcular totales
            order.SubtotalAmount -= itemSubtotal;
            order.TotalTaxAmount -= itemTaxAmount;
            order.TotalAmount -= itemTotal;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(order).Collection(o => o.Items).LoadAsync();
            foreach (var orderItem in order.Items)
            {
                await _dbContext.Entry(orderItem).Reference(i => i.ProductVariant).LoadAsync();
                await _dbContext.Entry(orderItem.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            }
            await _dbContext.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _dbContext.Entry(order.Customer).Reference(c => c.Contact).LoadAsync();
            await _dbContext.Entry(order).Reference(o => o.CreatedByUser).LoadAsync();

            return MapToDto(order);
        }

        public async Task<SalesOrderDto> FinalizeOrderAsync(Guid salesOrderId, Guid organizationId)
        {
            var order = await _dbContext.SalesOrders
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Contact)
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Wallet)
                .Include(so => so.CreatedByUser)
                .Include(so => so.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.OrganizationId == organizationId);

            if (order == null)
                throw new KeyNotFoundException("Orden no encontrada");

            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Solo se pueden finalizar órdenes en estado Draft");

            // Verificar que el cliente tenga fondos suficientes
            if (order.Customer.Wallet == null)
                throw new InvalidOperationException("El cliente no tiene una wallet asociada");

            if (order.Customer.Wallet.Balance < order.TotalAmount)
                throw new InvalidOperationException($"Fondos insuficientes. Balance: {order.Customer.Wallet.Balance:C}, Total orden: {order.TotalAmount:C}");

            if (!order.CreatedByUserId.HasValue)
                throw new InvalidOperationException("La orden no tiene un usuario creador asignado");

            // Verificar y registrar movimientos de stock para cada item
            // También capturar el costo promedio ponderado al momento de la venta
            foreach (var item in order.Items)
            {
                // Capturar el costo promedio actual del producto antes de la venta
                var variant = await _dbContext.ProductVariants
                    .FirstOrDefaultAsync(pv => pv.Id == item.ProductVariantId);

                if (variant != null)
                {
                    item.UnitCost = variant.AverageCost;
                }

                // RegisterSaleMovementAsync valida el stock y lo actualiza automáticamente
                await _stockMovementService.RegisterSaleMovementAsync(
                    organizationId,
                    order.CreatedByUserId.Value,
                    item.ProductVariantId,
                    item.Quantity,
                    salesOrderId);
            }

            // Descontar del wallet y crear transacción
            order.Customer.Wallet.Balance -= order.TotalAmount;
            order.Customer.Wallet.UpdatedAt = DateTime.UtcNow;

            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                WalletId = order.Customer.Wallet.Id,
                Type = TransactionType.Withdrawal,
                Amount = order.TotalAmount,
                RelatedSalesOrderId = salesOrderId,
                AuthorizedByUserId = order.CreatedByUserId,
                Notes = $"Compra - Orden #{salesOrderId.ToString()[..8]}",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WalletTransactions.Add(walletTransaction);

            // Cambiar estado de la orden
            order.Status = OrderStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(order);
        }

        public async Task<SalesOrderDto> CancelOrderAsync(Guid salesOrderId, Guid organizationId)
        {
            var order = await _dbContext.SalesOrders
                .Include(so => so.Customer)
                    .ThenInclude(c => c.Contact)
                .Include(so => so.CreatedByUser)
                .Include(so => so.Items)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId && so.OrganizationId == organizationId);

            if (order == null)
                throw new KeyNotFoundException("Orden no encontrada");

            if (order.Status != OrderStatus.Draft)
                throw new InvalidOperationException("Solo se pueden cancelar órdenes en estado Draft");

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return MapToDto(order);
        }

        private static SalesOrderDto MapToDto(SalesOrder order)
        {
            return new SalesOrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = $"{order.Customer.Contact.FirstName} {order.Customer.Contact.LastName}".Trim(),
                CustomerEmail = order.Customer.Contact.Email,
                CreatedByUserId = order.CreatedByUserId,
                CreatedByUserName = order.CreatedByUser != null
                    ? $"{order.CreatedByUser.FirstName} {order.CreatedByUser.LastName}".Trim()
                    : null,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                Items = order.Items?.Select(i => {
                    var subtotal = i.UnitPrice * i.Quantity;
                    var grossProfit = (i.UnitPrice - i.UnitCost) * i.Quantity;
                    var profitMargin = subtotal > 0 ? (grossProfit / subtotal) * 100 : 0;

                    return new SalesOrderItemDto
                    {
                        Id = i.Id,
                        ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductVariant.Product.Name,
                        VariantSku = i.ProductVariant.Sku,
                        VariantAttributes = i.ProductVariant.Attributes,
                        Quantity = i.Quantity,
                        OriginalPrice = i.OriginalPrice,
                        UnitPrice = i.UnitPrice,
                        UnitCost = i.UnitCost,
                        Subtotal = subtotal,
                        GrossProfit = grossProfit,
                        ProfitMargin = profitMargin,
                        ItemDescription = i.ItemDescription
                    };
                }).ToList() ?? new List<SalesOrderItemDto>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}
