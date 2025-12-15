using Microsoft.EntityFrameworkCore;
using Mapster;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Purchases;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    /// <summary>
    /// 📋 PurchaseOrderService - Gestiona órdenes de compra a proveedores
    /// </summary>
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly LiveSoldDbContext _context;
        private readonly ISerieNoService _serieNoService;

        public PurchaseOrderService(LiveSoldDbContext context, ISerieNoService serieNoService)
        {
            _context = context;
            _serieNoService = serieNoService;
        }

        /// <summary>
        /// Obtiene lista de órdenes de compra con filtros opcionales
        /// </summary>
        public async Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync(
            Guid organizationId,
            Guid? vendorId = null,
            string? status = null)
        {
            var query = _context.PurchaseOrders
                .Include(po => po.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(po => po.CreatedByUser)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .Include(po => po.Items)
                    .ThenInclude(i => i.ProductVariant)
                .Where(po => po.OrganizationId == organizationId);

            // Filtrar por proveedor
            if (vendorId.HasValue)
            {
                query = query.Where(po => po.VendorId == vendorId.Value);
            }

            // Filtrar por estado
            if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
            {
                if (Enum.TryParse<PurchaseOrderStatus>(status, true, out var orderStatus))
                {
                    query = query.Where(po => po.Status == orderStatus);
                }
            }

            var orders = await query
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return orders.Adapt<List<PurchaseOrderDto>>();
        }

        /// <summary>
        /// Obtiene una orden de compra por ID
        /// </summary>
        public async Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid orderId, Guid organizationId)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Vendor)
                    .ThenInclude(v => v.Contact)
                .Include(po => po.CreatedByUser)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .Include(po => po.Items)
                    .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(po => po.Id == orderId && po.OrganizationId == organizationId);

            return order.Adapt<PurchaseOrderDto>();
        }

        /// <summary>
        /// Crea una nueva orden de compra
        /// </summary>
        public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(
            Guid organizationId,
            Guid userId,
            CreatePurchaseOrderDto dto)
        {
            try
            {
                 // Validar que el proveedor existe
                var vendor = await _context.Vendors
                    .FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.OrganizationId == organizationId);

                if (vendor == null)
                    throw new Exception($"Proveedor con ID {dto.VendorId} no encontrado");

                // Validar que los productos existen
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && p.OrganizationId == organizationId)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    throw new Exception("Uno o más productos no existen");

                // Generar número de orden usando series numéricas
                var poNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.PurchaseOrder);

                var order = new PurchaseOrder
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    PONumber = poNumber,
                    VendorId = dto.VendorId,
                    OrderDate = DateTime.SpecifyKind(dto.OrderDate, DateTimeKind.Utc),
                    ExpectedDeliveryDate = dto.ExpectedDeliveryDate.HasValue
                        ? DateTime.SpecifyKind(dto.ExpectedDeliveryDate.Value, DateTimeKind.Utc)
                        : (DateTime?)null,
                    Status = PurchaseOrderStatus.Draft,
                    Subtotal = 0,
                    TaxAmount = 0,
                    TotalAmount = 0,
                    Currency = dto.Currency,
                    ExchangeRate = dto.ExchangeRate,
                    PaymentTermsId = dto.PaymentTermsId,
                    Notes = dto.Notes,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Crear items de la orden
                decimal subtotal = 0;
                decimal totalTaxAmount = 0;
                int lineNumber = 1;

                foreach (var itemDto in dto.Items)
                {
                    // Calcular totales de la línea
                    var lineSubtotal = itemDto.Quantity * itemDto.UnitCost;
                    var discountAmount = lineSubtotal * (itemDto.DiscountPercentage / 100);
                    var lineAfterDiscount = lineSubtotal - discountAmount;
                    var lineTaxAmount = lineAfterDiscount * (itemDto.TaxRate / 100);
                    var lineTotal = lineAfterDiscount + lineTaxAmount;

                    var item = new PurchaseOrderItem
                    {
                        Id = Guid.NewGuid(),
                        PurchaseOrderId = order.Id,
                        LineNumber = lineNumber++,
                        ProductId = itemDto.ProductId,
                        ProductVariantId = itemDto.ProductVariantId,
                        Description = itemDto.Description,
                        Quantity = itemDto.Quantity,
                        UnitCost = itemDto.UnitCost,
                        DiscountPercentage = itemDto.DiscountPercentage,
                        DiscountAmount = discountAmount,
                        TaxRate = itemDto.TaxRate,
                        TaxAmount = lineTaxAmount,
                        LineTotal = lineTotal,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    order.Items.Add(item);
                    subtotal += lineAfterDiscount;
                    totalTaxAmount += lineTaxAmount;
                }

                // Actualizar totales de la orden
                order.Subtotal = subtotal;
                order.TaxAmount = totalTaxAmount;
                order.TotalAmount = subtotal + totalTaxAmount;

                _context.PurchaseOrders.Add(order);
                await _context.SaveChangesAsync();

                return await GetPurchaseOrderByIdAsync(order.Id, organizationId)
                    ?? throw new Exception("Error al recuperar la orden de compra creada");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Actualiza el estado de una orden de compra
        /// </summary>
        public async Task<PurchaseOrderDto> UpdatePurchaseOrderStatusAsync(
            Guid orderId,
            Guid organizationId,
            string status)
        {
            var order = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == orderId && po.OrganizationId == organizationId);

            if (order == null)
                throw new Exception($"Orden de compra con ID {orderId} no encontrada");

            if (!Enum.TryParse<PurchaseOrderStatus>(status, true, out var orderStatus))
                throw new Exception($"Estado '{status}' no es válido");

            order.Status = orderStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetPurchaseOrderByIdAsync(order.Id, organizationId)
                ?? throw new Exception("Error al recuperar la orden actualizada");
        }

        /// <summary>
        /// Elimina una orden de compra (solo si está en estado Draft)
        /// </summary>
        public async Task DeletePurchaseOrderAsync(Guid orderId, Guid organizationId)
        {
            var order = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == orderId && po.OrganizationId == organizationId);

            if (order == null)
                throw new Exception($"Orden de compra con ID {orderId} no encontrada");

            if (order.Status != PurchaseOrderStatus.Draft)
                throw new Exception("Solo se pueden eliminar órdenes en estado Draft");

            _context.PurchaseOrders.Remove(order);
            await _context.SaveChangesAsync();
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Genera un número secuencial para la orden de compra
        /// </summary>
        private async Task<string> GeneratePONumberAsync(Guid organizationId)
        {
            var year = DateTime.UtcNow.Year;
            var lastOrder = await _context.PurchaseOrders
                .Where(po => po.OrganizationId == organizationId && po.PONumber.StartsWith($"PO-{year}-"))
                .OrderByDescending(po => po.PONumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastOrder != null)
            {
                var parts = lastOrder.PONumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"PO-{year}-{nextNumber:D6}";
        }

        #endregion
    }
}
