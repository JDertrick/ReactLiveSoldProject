using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly LiveSoldDbContext _dbContext;

        public StockMovementService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<StockMovementDto>> GetMovementsByVariantAsync(Guid productVariantId, Guid organizationId)
        {
            var movements = await _dbContext.StockMovements
                .Include(sm => sm.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(sm => sm.CreatedByUser)
                .Where(sm => sm.ProductVariantId == productVariantId && sm.OrganizationId == organizationId)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();

            return movements.Select(m => MapToDto(m)).ToList();
        }

        public async Task<List<StockMovementDto>> GetMovementsByOrganizationAsync(Guid organizationId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbContext.StockMovements
                .Include(sm => sm.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(sm => sm.CreatedByUser)
                .Where(sm => sm.OrganizationId == organizationId);

            if (fromDate.HasValue)
                query = query.Where(sm => sm.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sm => sm.CreatedAt <= toDate.Value);

            var movements = await query
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();

            return movements.Select(m => MapToDto(m)).ToList();
        }

        public async Task<StockMovementDto> CreateMovementAsync(Guid organizationId, Guid userId, CreateStockMovementDto dto)
        {
            // Validar que el tipo de movimiento sea válido
            if (!Enum.TryParse<StockMovementType>(dto.MovementType, out var movementType))
                throw new InvalidOperationException($"Tipo de movimiento inválido: {dto.MovementType}");

            // No permitir movimientos de venta desde este método (usar RegisterSaleMovementAsync)
            if (movementType == StockMovementType.Sale)
                throw new InvalidOperationException("Los movimientos de venta deben ser registrados a través del proceso de ventas");

            // Obtener la variante del producto
            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == dto.ProductVariantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante de producto no encontrada");

            var stockBefore = variant.StockQuantity;

            if (movementType == StockMovementType.Return || movementType == StockMovementType.Loss)
                dto.Quantity = dto.Quantity * -1;
                
            var stockAfter = stockBefore + dto.Quantity; // Puede ser positivo o negativo

            // Validar que el stock no sea negativo
            if (stockAfter < 0)
                throw new InvalidOperationException($"Stock insuficiente. Stock actual: {stockBefore}, Cantidad solicitada: {Math.Abs(dto.Quantity)}");

            // Crear el movimiento
            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductVariantId = dto.ProductVariantId,
                MovementType = movementType,
                Quantity = dto.Quantity,
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                CreatedByUserId = userId,
                Notes = dto.Notes,
                Reference = dto.Reference,
                UnitCost = dto.UnitCost,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StockMovements.Add(movement);

            // Actualizar el stock en la variante
            variant.StockQuantity = stockAfter;
            variant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(movement).Reference(m => m.ProductVariant).LoadAsync();
            await _dbContext.Entry(movement.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.CreatedByUser).LoadAsync();

            return MapToDto(movement);
        }

        public async Task RegisterSaleMovementAsync(Guid organizationId, Guid userId, Guid productVariantId, int quantity, Guid salesOrderId)
        {
            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == productVariantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante de producto no encontrada");

            var stockBefore = variant.StockQuantity;
            var stockAfter = stockBefore - quantity; // Resta para ventas

            if (stockAfter < 0)
                throw new InvalidOperationException($"Stock insuficiente. Stock actual: {stockBefore}, Cantidad solicitada: {quantity}");

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductVariantId = productVariantId,
                MovementType = StockMovementType.Sale,
                Quantity = -quantity, // Negativo para ventas
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                RelatedSalesOrderId = salesOrderId,
                CreatedByUserId = userId,
                Notes = $"Venta - Orden #{salesOrderId.ToString()[..8]}",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StockMovements.Add(movement);

            // Actualizar el stock en la variante
            variant.StockQuantity = stockAfter;
            variant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task RegisterSaleCancellationMovementAsync(Guid organizationId, Guid userId, Guid productVariantId, int quantity, Guid salesOrderId)
        {
            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == productVariantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante de producto no encontrada");

            var stockBefore = variant.StockQuantity;
            var stockAfter = stockBefore + quantity; // Suma para devoluciones

            var movement = new StockMovement
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductVariantId = productVariantId,
                MovementType = StockMovementType.SaleCancellation,
                Quantity = quantity, // Positivo para cancelaciones
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                RelatedSalesOrderId = salesOrderId,
                CreatedByUserId = userId,
                Notes = $"Cancelación de venta - Orden #{salesOrderId.ToString()[..8]}",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StockMovements.Add(movement);

            // Actualizar el stock en la variante
            variant.StockQuantity = stockAfter;
            variant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCurrentStockAsync(Guid productVariantId, Guid organizationId)
        {
            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == productVariantId && pv.OrganizationId == organizationId);

            return variant?.StockQuantity ?? 0;
        }

        private static StockMovementDto MapToDto(StockMovement movement)
        {
            return new StockMovementDto
            {
                Id = movement.Id,
                ProductVariantId = movement.ProductVariantId,
                ProductName = movement.ProductVariant?.Product?.Name ?? "N/A",
                VariantSku = movement.ProductVariant?.Sku ?? "N/A",
                MovementType = movement.MovementType.ToString(),
                Quantity = movement.Quantity,
                StockBefore = movement.StockBefore,
                StockAfter = movement.StockAfter,
                RelatedSalesOrderId = movement.RelatedSalesOrderId,
                CreatedByUserName = movement.CreatedByUser != null
                    ? $"{movement.CreatedByUser.FirstName} {movement.CreatedByUser.LastName}".Trim()
                    : null,
                Notes = movement.Notes,
                Reference = movement.Reference,
                UnitCost = movement.UnitCost,
                CreatedAt = movement.CreatedAt
            };
        }
    }
}
