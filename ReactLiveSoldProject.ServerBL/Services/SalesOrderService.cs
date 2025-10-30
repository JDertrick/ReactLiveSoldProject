using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Sales;

namespace ReactLiveSoldProject.ServerBL.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly LiveSoldDbContext _dbContext;

        public SalesOrderService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SalesOrderDto>> GetSalesOrdersByOrganizationAsync(Guid organizationId, string? status = null)
        {
            var query = _dbContext.SalesOrders
                .Include(so => so.Customer)
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

            foreach (var itemDto in dto.Items)
            {
                var variant = await _dbContext.ProductVariants
                    .Include(pv => pv.Product)
                    .FirstOrDefaultAsync(pv => pv.Id == itemDto.ProductVariantId && pv.OrganizationId == organizationId);

                if (variant == null)
                    throw new KeyNotFoundException($"Variante de producto no encontrada: {itemDto.ProductVariantId}");

                var unitPrice = itemDto.CustomUnitPrice ?? variant.Price;
                var subtotal = unitPrice * itemDto.Quantity;

                var orderItem = new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    SalesOrderId = orderId,
                    ProductVariantId = itemDto.ProductVariantId,
                    Quantity = itemDto.Quantity,
                    OriginalPrice = variant.Price,
                    UnitPrice = unitPrice,
                    ItemDescription = itemDto.ItemDescription
                };

                _dbContext.SalesOrderItems.Add(orderItem);
                totalAmount += subtotal;
            }

            order.TotalAmount = totalAmount;

            await _dbContext.SaveChangesAsync();

            // Recargar la orden con sus relaciones
            var createdOrder = await _dbContext.SalesOrders
                .Include(so => so.Customer)
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
                .FirstOrDefaultAsync(pv => pv.Id == dto.ProductVariantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante de producto no encontrada");

            var unitPrice = dto.CustomUnitPrice ?? variant.Price;
            var subtotal = unitPrice * dto.Quantity;

            var orderItem = new SalesOrderItem
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                SalesOrderId = salesOrderId,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity,
                OriginalPrice = variant.Price,
                UnitPrice = unitPrice,
                ItemDescription = dto.ItemDescription
            };

            _dbContext.SalesOrderItems.Add(orderItem);

            // Recalcular total
            order.TotalAmount += subtotal;
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

            var subtotal = item.UnitPrice * item.Quantity;

            _dbContext.SalesOrderItems.Remove(item);

            // Recalcular total
            order.TotalAmount -= subtotal;
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
            await _dbContext.Entry(order).Reference(o => o.CreatedByUser).LoadAsync();

            return MapToDto(order);
        }

        public async Task<SalesOrderDto> FinalizeOrderAsync(Guid salesOrderId, Guid organizationId)
        {
            var order = await _dbContext.SalesOrders
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

            // Verificar y descontar stock de cada item
            foreach (var item in order.Items)
            {
                if (item.ProductVariant.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para {item.ProductVariant.Product.Name} ({item.ProductVariant.Sku}). Disponible: {item.ProductVariant.StockQuantity}, Requerido: {item.Quantity}");

                item.ProductVariant.StockQuantity -= item.Quantity;
                item.ProductVariant.UpdatedAt = DateTime.UtcNow;
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
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}".Trim(),
                CustomerEmail = order.Customer.Email,
                CreatedByUserId = order.CreatedByUserId,
                CreatedByUserName = order.CreatedByUser != null
                    ? $"{order.CreatedByUser.FirstName} {order.CreatedByUser.LastName}".Trim()
                    : null,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                Items = order.Items?.Select(i => new SalesOrderItemDto
                {
                    Id = i.Id,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant.Product.Name,
                    VariantSku = i.ProductVariant.Sku,
                    VariantAttributes = i.ProductVariant.Attributes,
                    Quantity = i.Quantity,
                    OriginalPrice = i.OriginalPrice,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.UnitPrice * i.Quantity,
                    ItemDescription = i.ItemDescription
                }).ToList() ?? new List<SalesOrderItemDto>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}
