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
                .Include(sm => sm.PostedByUser)
                .Include(sm => sm.SourceLocation)
                .Include(sm => sm.DestinationLocation)
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
                .Include(sm => sm.PostedByUser)
                .Include(sm => sm.SourceLocation)
                .Include(sm => sm.DestinationLocation)
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

            var stockAfter = stockBefore + dto.Quantity;

            // Validar que el stock no sea negativo después del posteo
            if (stockAfter < 0)
                throw new InvalidOperationException($"Stock insuficiente. Stock actual: {stockBefore}, Cantidad solicitada: {Math.Abs(dto.Quantity)}");

            // Validar que las entradas con costo positivo tengan un UnitCost
            if (dto.Quantity > 0 && movementType == StockMovementType.Purchase && !dto.UnitCost.HasValue)
                throw new InvalidOperationException("Las compras deben incluir un costo unitario");

            // Validar ubicaciones para transferencias
            if (movementType == StockMovementType.Transfer)
            {
                if (!dto.SourceLocationId.HasValue || !dto.DestinationLocationId.HasValue)
                    throw new InvalidOperationException("Las transferencias requieren ubicación de origen y destino");

                if (dto.SourceLocationId == dto.DestinationLocationId)
                    throw new InvalidOperationException("La ubicación de origen y destino no pueden ser la misma");

                // Validar que las ubicaciones existan
                var sourceLocationExists = await _dbContext.Locations.AnyAsync(l => l.Id == dto.SourceLocationId.Value && l.OrganizationId == organizationId);
                var destLocationExists = await _dbContext.Locations.AnyAsync(l => l.Id == dto.DestinationLocationId.Value && l.OrganizationId == organizationId);

                if (!sourceLocationExists || !destLocationExists)
                    throw new InvalidOperationException("Una o ambas ubicaciones no existen");
            }

            // Crear el movimiento en estado borrador (IsPosted = false)
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
                SourceLocationId = dto.SourceLocationId,
                DestinationLocationId = dto.DestinationLocationId,
                IsPosted = false, // Se crea como borrador
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.StockMovements.Add(movement);
            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(movement).Reference(m => m.ProductVariant).LoadAsync();
            await _dbContext.Entry(movement.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.CreatedByUser).LoadAsync();

            if (movement.SourceLocationId.HasValue)
                await _dbContext.Entry(movement).Reference(m => m.SourceLocation).LoadAsync();

            if (movement.DestinationLocationId.HasValue)
                await _dbContext.Entry(movement).Reference(m => m.DestinationLocation).LoadAsync();

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
                IsPosted = true, // Las ventas se postean automáticamente
                PostedAt = DateTime.UtcNow,
                PostedByUserId = userId,
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
                IsPosted = true, // Las cancelaciones se postean automáticamente
                PostedAt = DateTime.UtcNow,
                PostedByUserId = userId,
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

        /// <summary>
        /// Postea un movimiento de inventario, actualizando el stock y el costo promedio
        /// </summary>
        public async Task<StockMovementDto> PostMovementAsync(Guid movementId, Guid organizationId, Guid userId)
        {
            var movement = await _dbContext.StockMovements
                .Include(sm => sm.ProductVariant)
                .FirstOrDefaultAsync(sm => sm.Id == movementId && sm.OrganizationId == organizationId);

            if (movement == null)
                throw new KeyNotFoundException("Movimiento no encontrado");

            if (movement.IsPosted)
                throw new InvalidOperationException("El movimiento ya ha sido posteado");

            var variant = movement.ProductVariant;
            var stockBefore = variant.StockQuantity;
            var stockAfter = stockBefore + movement.Quantity;

            // Validar que el stock no sea negativo
            if (stockAfter < 0)
                throw new InvalidOperationException($"Stock insuficiente para postear. Stock actual: {stockBefore}, Cantidad del movimiento: {movement.Quantity}");

            // Actualizar stock
            variant.StockQuantity = stockAfter;

            // Calcular costo promedio ponderado para movimientos de entrada con costo
            if (movement.Quantity > 0 && movement.UnitCost.HasValue && movement.UnitCost.Value > 0)
            {
                // Fórmula: (Stock Anterior × Costo Anterior + Cantidad Nueva × Costo Nuevo) / (Stock Anterior + Cantidad Nueva)
                var totalCostBefore = stockBefore * variant.AverageCost;
                var totalCostNew = movement.Quantity * movement.UnitCost.Value;
                var totalCostAfter = totalCostBefore + totalCostNew;

                variant.AverageCost = stockAfter > 0 ? totalCostAfter / stockAfter : 0;
            }

            // Marcar como posteado
            movement.IsPosted = true;
            movement.PostedAt = DateTime.UtcNow;
            movement.PostedByUserId = userId;

            // Actualizar StockBefore y StockAfter con valores reales al momento del posteo
            movement.StockBefore = stockBefore;
            movement.StockAfter = stockAfter;

            variant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(movement).Reference(m => m.ProductVariant).LoadAsync();
            await _dbContext.Entry(movement.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.CreatedByUser).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.PostedByUser).LoadAsync();

            return MapToDto(movement);
        }

        /// <summary>
        /// Despostea un movimiento de inventario (solo si es el último movimiento posteado para esa variante)
        /// </summary>
        public async Task<StockMovementDto> UnpostMovementAsync(Guid movementId, Guid organizationId)
        {
            var movement = await _dbContext.StockMovements
                .Include(sm => sm.ProductVariant)
                .FirstOrDefaultAsync(sm => sm.Id == movementId && sm.OrganizationId == organizationId);

            if (movement == null)
                throw new KeyNotFoundException("Movimiento no encontrado");

            if (!movement.IsPosted)
                throw new InvalidOperationException("El movimiento no está posteado");

            // Verificar que sea el último movimiento posteado para esta variante
            var lastPostedMovement = await _dbContext.StockMovements
                .Where(sm => sm.ProductVariantId == movement.ProductVariantId && sm.IsPosted)
                .OrderByDescending(sm => sm.PostedAt)
                .FirstOrDefaultAsync();

            if (lastPostedMovement?.Id != movementId)
                throw new InvalidOperationException("Solo se puede despostear el último movimiento posteado. Despostee los movimientos más recientes primero.");

            var variant = movement.ProductVariant;

            // Revertir el stock
            variant.StockQuantity = movement.StockBefore;

            // Recalcular el costo promedio revirtiendo el movimiento
            // Para esto, necesitamos obtener el costo promedio anterior
            // Buscar el movimiento anterior posteado con costo
            var previousMovementWithCost = await _dbContext.StockMovements
                .Where(sm => sm.ProductVariantId == movement.ProductVariantId
                    && sm.IsPosted
                    && sm.PostedAt < movement.PostedAt
                    && sm.UnitCost.HasValue)
                .OrderByDescending(sm => sm.PostedAt)
                .FirstOrDefaultAsync();

            if (previousMovementWithCost != null)
            {
                // Recalcular desde el principio hasta el movimiento anterior
                var allMovementsUntilPrevious = await _dbContext.StockMovements
                    .Where(sm => sm.ProductVariantId == movement.ProductVariantId
                        && sm.IsPosted
                        && sm.PostedAt <= previousMovementWithCost.PostedAt)
                    .OrderBy(sm => sm.PostedAt)
                    .ToListAsync();

                decimal recalculatedCost = 0;
                int recalculatedStock = 0;

                foreach (var m in allMovementsUntilPrevious)
                {
                    if (m.Quantity > 0 && m.UnitCost.HasValue && m.UnitCost.Value > 0)
                    {
                        var totalCostBefore = recalculatedStock * recalculatedCost;
                        var totalCostNew = m.Quantity * m.UnitCost.Value;
                        recalculatedStock += m.Quantity;
                        recalculatedCost = recalculatedStock > 0 ? (totalCostBefore + totalCostNew) / recalculatedStock : 0;
                    }
                    else
                    {
                        recalculatedStock += m.Quantity;
                    }
                }

                variant.AverageCost = recalculatedCost;
            }
            else
            {
                variant.AverageCost = 0;
            }

            // Desmarcar como posteado
            movement.IsPosted = false;
            movement.PostedAt = null;
            movement.PostedByUserId = null;

            variant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones
            await _dbContext.Entry(movement).Reference(m => m.ProductVariant).LoadAsync();
            await _dbContext.Entry(movement.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.CreatedByUser).LoadAsync();

            return MapToDto(movement);
        }

        public async Task<StockMovementDto> RejectMovementAsync(Guid movementId, Guid organizationId, Guid userId)
        {
            var movement = await _dbContext.StockMovements
                .FirstOrDefaultAsync(sm => sm.Id == movementId && sm.OrganizationId == organizationId);

            if (movement == null)
                throw new KeyNotFoundException("Movimiento no encontrado");

            if (movement.IsPosted || movement.IsRejected)
                throw new InvalidOperationException("El movimiento ya ha sido procesado (posteado o rechazado).");

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || !await _dbContext.OrganizationMembers.AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userId))
                throw new UnauthorizedAccessException("El usuario no está autorizado para realizar esta acción.");

            movement.IsRejected = true;
            movement.RejectedAt = DateTime.UtcNow;
            movement.RejectedByUserId = userId;

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(movement).Reference(m => m.ProductVariant).LoadAsync();
            await _dbContext.Entry(movement.ProductVariant).Reference(pv => pv.Product).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.CreatedByUser).LoadAsync();
            await _dbContext.Entry(movement).Reference(m => m.RejectedByUser).LoadAsync();

            return MapToDto(movement);
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
                SourceLocationId = movement.SourceLocationId,
                SourceLocation = movement.SourceLocation != null ? new LocationDto
                {
                    Id = movement.SourceLocation.Id,
                    Name = movement.SourceLocation.Name,
                    Description = movement.SourceLocation.Description,
                    OrganizationId = movement.SourceLocation.OrganizationId
                } : null,
                DestinationLocationId = movement.DestinationLocationId,
                DestinationLocation = movement.DestinationLocation != null ? new LocationDto
                {
                    Id = movement.DestinationLocation.Id,
                    Name = movement.DestinationLocation.Name,
                    Description = movement.DestinationLocation.Description,
                    OrganizationId = movement.DestinationLocation.OrganizationId
                } : null,
                IsPosted = movement.IsPosted,
                PostedAt = movement.PostedAt,
                PostedByUserName = movement.PostedByUser != null
                    ? $"{movement.PostedByUser.FirstName} {movement.PostedByUser.LastName}".Trim()
                    : null,
                IsRejected = movement.IsRejected,
                RejectedAt = movement.RejectedAt,
                RejectedByUserName = movement.RejectedByUser != null
                    ? $"{movement.RejectedByUser.FirstName} {movement.RejectedByUser.LastName}".Trim()
                    : null,
                CreatedAt = movement.CreatedAt
            };
        }
    }
}
