using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class InventoryAuditService : IInventoryAuditService
    {
        private readonly LiveSoldDbContext _context;

        public InventoryAuditService(LiveSoldDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryAuditDto> CreateAuditAsync(Guid organizationId, Guid userId, CreateInventoryAuditDto dto)
        {
            // Query base de variantes activas
            var query = _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.TagLinks)
                        .ThenInclude(tl => tl.Tag)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Where(v => v.OrganizationId == organizationId && v.Product.IsPublished);

            // Construir descripción del scope
            var scopeParts = new List<string>();

            // Aplicar filtros según el tipo de auditoría
            if (dto.ScopeType == DTOs.AuditScopeType.Partial)
            {
                // Filtro por categorías
                if (dto.CategoryIds != null && dto.CategoryIds.Any())
                {
                    query = query.Where(v => v.Product.CategoryId.HasValue && dto.CategoryIds.Contains(v.Product.CategoryId.Value));
                    var categoryNames = await _context.Categories
                        .Where(c => dto.CategoryIds.Contains(c.Id))
                        .Select(c => c.Name)
                        .ToListAsync();
                    scopeParts.Add($"Categorías: {string.Join(", ", categoryNames)}");
                }

                // Filtro por tags/proveedores
                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    query = query.Where(v => v.Product.TagLinks.Any(tl => dto.TagIds.Contains(tl.TagId)));
                    var tagNames = await _context.Tags
                        .Where(t => dto.TagIds.Contains(t.Id))
                        .Select(t => t.Name)
                        .ToListAsync();
                    scopeParts.Add($"Tags: {string.Join(", ", tagNames)}");
                }
            }

            // Excluir productos auditados recientemente
            if (dto.ExcludeAuditedInLastDays > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-dto.ExcludeAuditedInLastDays);
                var recentlyAuditedVariantIds = await _context.InventoryAuditItems
                    .Include(i => i.InventoryAudit)
                    .Where(i => i.InventoryAudit.OrganizationId == organizationId
                        && i.InventoryAudit.Status == InventoryAuditStatus.Completed
                        && i.InventoryAudit.CompletedAt >= cutoffDate)
                    .Select(i => i.ProductVariantId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(v => !recentlyAuditedVariantIds.Contains(v.Id));
                scopeParts.Add($"No auditados en últimos {dto.ExcludeAuditedInLastDays} días");
            }

            var variants = await query.ToListAsync();

            // Aplicar selección aleatoria si se especifica
            if (dto.RandomSampleCount > 0 && variants.Count > dto.RandomSampleCount)
            {
                var random = new Random();
                variants = variants.OrderBy(x => random.Next()).Take(dto.RandomSampleCount).ToList();
                scopeParts.Add($"Muestra aleatoria: {dto.RandomSampleCount} items");
            }

            if (!variants.Any())
                throw new InvalidOperationException("No hay variantes de producto que coincidan con los filtros especificados");

            // Obtener información de ubicación si se especifica
            Location? location = null;
            if (dto.LocationId.HasValue)
            {
                location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId.Value);
                if (location != null)
                {
                    scopeParts.Insert(0, $"Bodega: {location.Name}");
                }
            }

            var scopeDescription = scopeParts.Any() ? string.Join(" | ", scopeParts) : "Auditoría Total";

            var audit = new InventoryAudit
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = dto.Name,
                Description = dto.Description,
                Notes = dto.Notes,
                Status = InventoryAuditStatus.Draft,
                SnapshotTakenAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                TotalVariants = variants.Count,
                CountedVariants = 0,
                TotalVariance = 0,
                TotalVarianceValue = 0,
                // Scope fields
                ScopeType = dto.ScopeType.ToString(),
                LocationId = dto.LocationId,
                ScopeDescription = scopeDescription
            };

            // Crear items con snapshot del stock actual
            foreach (var variant in variants)
            {
                var item = new InventoryAuditItem
                {
                    Id = Guid.NewGuid(),
                    InventoryAuditId = audit.Id,
                    ProductVariantId = variant.Id,
                    TheoreticalStock = variant.StockQuantity,
                    SnapshotAverageCost = variant.AverageCost,
                    CountedStock = null,
                    Variance = null,
                    VarianceValue = null
                };
                audit.Items.Add(item);
            }

            _context.InventoryAudits.Add(audit);
            await _context.SaveChangesAsync();

            // Reload with Location for mapping
            if (location != null) audit.Location = location;

            return await MapToDto(audit);
        }

        public async Task<InventoryAuditDetailDto?> GetAuditByIdAsync(Guid auditId, Guid organizationId)
        {
            var audit = await _context.InventoryAudits
                .Include(a => a.CreatedByUser)
                .Include(a => a.CompletedByUser)
                .Include(a => a.Items)
                    .ThenInclude(i => i.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(a => a.Items)
                    .ThenInclude(i => i.CountedByUser)
                .Where(a => a.Id == auditId && a.OrganizationId == organizationId)
                .FirstOrDefaultAsync();

            if (audit == null) return null;

            return new InventoryAuditDetailDto
            {
                Id = audit.Id,
                OrganizationId = audit.OrganizationId,
                Name = audit.Name,
                Description = audit.Description,
                Status = audit.Status.ToString(),
                SnapshotTakenAt = audit.SnapshotTakenAt,
                StartedAt = audit.StartedAt,
                CompletedAt = audit.CompletedAt,
                CreatedByUserId = audit.CreatedByUserId,
                CreatedByUserName = audit.CreatedByUser?.FirstName + " " + audit.CreatedByUser?.LastName,
                CompletedByUserId = audit.CompletedByUserId,
                CompletedByUserName = audit.CompletedByUser != null
                    ? audit.CompletedByUser.FirstName + " " + audit.CompletedByUser.LastName
                    : null,
                TotalVariants = audit.TotalVariants,
                CountedVariants = audit.CountedVariants,
                TotalVariance = audit.TotalVariance,
                TotalVarianceValue = audit.TotalVarianceValue,
                Notes = audit.Notes,
                CreatedAt = audit.CreatedAt,
                UpdatedAt = audit.UpdatedAt,
                Items = audit.Items.Select(i => MapItemToDto(i)).ToList()
            };
        }

        public async Task<List<InventoryAuditDto>> GetAuditsByOrganizationAsync(Guid organizationId, string? status = null)
        {
            var query = _context.InventoryAudits
                .Include(a => a.CreatedByUser)
                .Include(a => a.CompletedByUser)
                .Include(a => a.Location)
                .Where(a => a.OrganizationId == organizationId);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<InventoryAuditStatus>(status, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }

            var audits = await query.OrderByDescending(a => a.CreatedAt).ToListAsync();

            var result = new List<InventoryAuditDto>();
            foreach (var audit in audits)
            {
                result.Add(await MapToDto(audit));
            }
            return result;
        }

        public async Task<InventoryAuditDto> StartAuditAsync(Guid auditId, Guid organizationId, Guid userId)
        {
            var audit = await _context.InventoryAudits
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            if (audit.Status != InventoryAuditStatus.Draft)
                throw new InvalidOperationException("Solo se pueden iniciar auditorías en estado borrador");

            audit.Status = InventoryAuditStatus.InProgress;
            audit.StartedAt = DateTime.UtcNow;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await MapToDto(audit);
        }

        public async Task<List<BlindCountItemDto>> GetBlindCountItemsAsync(Guid auditId, Guid organizationId)
        {
            var audit = await _context.InventoryAudits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            var items = await _context.InventoryAuditItems
                .Include(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Where(i => i.InventoryAuditId == auditId)
                .OrderBy(i => i.ProductVariant.Product.Name)
                .ThenBy(i => i.ProductVariant.Sku)
                .ToListAsync();

            return items.Select(i => new BlindCountItemDto
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductVariant.Product.Name,
                VariantSku = i.ProductVariant.Sku ?? "",
                VariantSize = i.ProductVariant.Size,
                VariantColor = i.ProductVariant.Color,
                VariantImageUrl = i.ProductVariant.ImageUrl,
                CountedStock = i.CountedStock,
                Notes = i.Notes
            }).ToList();
        }

        public async Task<List<InventoryAuditItemDto>> GetAuditItemsAsync(Guid auditId, Guid organizationId)
        {
            var audit = await _context.InventoryAudits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            var items = await _context.InventoryAuditItems
                .Include(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Include(i => i.CountedByUser)
                .Where(i => i.InventoryAuditId == auditId)
                .OrderBy(i => i.ProductVariant.Product.Name)
                .ThenBy(i => i.ProductVariant.Sku)
                .ToListAsync();

            return items.Select(i => MapItemToDto(i)).ToList();
        }

        public async Task<InventoryAuditItemDto> UpdateCountAsync(Guid auditId, Guid organizationId, Guid userId, UpdateAuditCountDto dto)
        {
            var audit = await _context.InventoryAudits
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            if (audit.Status == InventoryAuditStatus.Completed)
                throw new InvalidOperationException("No se puede modificar una auditoría completada");

            if (audit.Status == InventoryAuditStatus.Cancelled)
                throw new InvalidOperationException("No se puede modificar una auditoría cancelada");

            var item = await _context.InventoryAuditItems
                .Include(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
                .Include(i => i.CountedByUser)
                .FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.InventoryAuditId == auditId);

            if (item == null)
                throw new KeyNotFoundException("Item de auditoría no encontrado");

            // Actualizar si es la primera vez que se cuenta
            bool wasNotCounted = !item.CountedStock.HasValue;

            item.CountedStock = dto.CountedStock;
            item.Variance = dto.CountedStock - item.TheoreticalStock;
            item.VarianceValue = item.Variance * item.SnapshotAverageCost;
            item.CountedByUserId = userId;
            item.CountedAt = DateTime.UtcNow;
            item.Notes = dto.Notes;
            item.UpdatedAt = DateTime.UtcNow;

            // Actualizar contadores de la auditoría
            if (wasNotCounted)
            {
                audit.CountedVariants++;
            }

            // Recalcular totales
            await RecalculateAuditTotals(audit);

            await _context.SaveChangesAsync();
            return MapItemToDto(item);
        }

        public async Task<List<InventoryAuditItemDto>> BulkUpdateCountAsync(Guid auditId, Guid organizationId, Guid userId, BulkUpdateAuditCountDto dto)
        {
            var results = new List<InventoryAuditItemDto>();
            foreach (var count in dto.Counts)
            {
                var result = await UpdateCountAsync(auditId, organizationId, userId, count);
                results.Add(result);
            }
            return results;
        }

        public async Task<InventoryAuditSummaryDto> GetAuditSummaryAsync(Guid auditId, Guid organizationId)
        {
            var audit = await _context.InventoryAudits
                .Include(a => a.Items)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            var countedItems = audit.Items.Where(i => i.CountedStock.HasValue).ToList();
            var itemsWithVariance = countedItems.Where(i => i.Variance != 0).ToList();
            var positiveVariance = countedItems.Where(i => i.Variance > 0).ToList();
            var negativeVariance = countedItems.Where(i => i.Variance < 0).ToList();

            return new InventoryAuditSummaryDto
            {
                AuditId = audit.Id,
                AuditName = audit.Name,
                Status = audit.Status.ToString(),
                TotalVariants = audit.TotalVariants,
                CountedVariants = audit.CountedVariants,
                PendingVariants = audit.TotalVariants - audit.CountedVariants,
                Progress = audit.TotalVariants > 0 ? (double)audit.CountedVariants / audit.TotalVariants * 100 : 0,

                ItemsWithVariance = itemsWithVariance.Count,
                ItemsWithPositiveVariance = positiveVariance.Count,
                ItemsWithNegativeVariance = negativeVariance.Count,
                ItemsWithNoVariance = countedItems.Count - itemsWithVariance.Count,

                TotalPositiveVariance = positiveVariance.Sum(i => i.Variance ?? 0),
                TotalNegativeVariance = negativeVariance.Sum(i => i.Variance ?? 0),
                NetVariance = audit.TotalVariance,

                TotalPositiveVarianceValue = positiveVariance.Sum(i => i.VarianceValue ?? 0),
                TotalNegativeVarianceValue = negativeVariance.Sum(i => i.VarianceValue ?? 0),
                NetVarianceValue = audit.TotalVarianceValue
            };
        }

        public async Task<InventoryAuditDto> CompleteAuditAsync(Guid auditId, Guid organizationId, Guid userId, CompleteAuditDto dto)
        {
            var audit = await _context.InventoryAudits
                .Include(a => a.Items)
                    .ThenInclude(i => i.ProductVariant)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            if (audit.Status == InventoryAuditStatus.Completed)
                throw new InvalidOperationException("La auditoría ya está completada");

            if (audit.Status == InventoryAuditStatus.Cancelled)
                throw new InvalidOperationException("No se puede completar una auditoría cancelada");

            // Verificar que todos los items estén contados
            var uncountedItems = audit.Items.Where(i => !i.CountedStock.HasValue).Count();
            if (uncountedItems > 0)
                throw new InvalidOperationException($"Hay {uncountedItems} items sin contar. Complete el conteo antes de finalizar.");

            // Generar movimientos de ajuste para items con varianza
            var itemsWithVariance = audit.Items.Where(i => i.Variance != 0).ToList();

            foreach (var item in itemsWithVariance)
            {
                var movement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    ProductVariantId = item.ProductVariantId,
                    MovementType = StockMovementType.InventoryAudit,
                    Quantity = item.Variance!.Value,
                    StockBefore = item.TheoreticalStock,
                    StockAfter = item.CountedStock!.Value,
                    CreatedByUserId = userId,
                    Notes = $"Ajuste por auditoría: {audit.Name}",
                    Reference = $"AUDIT-{audit.Id.ToString().Substring(0, 8)}",
                    IsPosted = false,
                    IsRejected = false,
                    CreatedAt = DateTime.UtcNow
                };

                // Si se solicita auto-postear
                if (dto.AutoPostAdjustments)
                {
                    movement.IsPosted = true;
                    movement.PostedAt = DateTime.UtcNow;
                    movement.PostedByUserId = userId;

                    // Actualizar el stock de la variante
                    item.ProductVariant.StockQuantity = item.CountedStock!.Value;
                    item.ProductVariant.UpdatedAt = DateTime.UtcNow;
                }

                _context.StockMovements.Add(movement);
                item.AdjustmentMovementId = movement.Id;
            }

            // Actualizar auditoría
            audit.Status = InventoryAuditStatus.Completed;
            audit.CompletedAt = DateTime.UtcNow;
            audit.CompletedByUserId = userId;
            if (!string.IsNullOrEmpty(dto.Notes))
            {
                audit.Notes = (audit.Notes ?? "") + "\n" + dto.Notes;
            }
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await MapToDto(audit);
        }

        public async Task<InventoryAuditDto> CancelAuditAsync(Guid auditId, Guid organizationId, Guid userId)
        {
            var audit = await _context.InventoryAudits
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == auditId && a.OrganizationId == organizationId);

            if (audit == null)
                throw new KeyNotFoundException("Auditoría no encontrada");

            if (audit.Status == InventoryAuditStatus.Completed)
                throw new InvalidOperationException("No se puede cancelar una auditoría completada");

            audit.Status = InventoryAuditStatus.Cancelled;
            audit.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await MapToDto(audit);
        }

        #region Private Methods

        private async Task RecalculateAuditTotals(InventoryAudit audit)
        {
            var items = await _context.InventoryAuditItems
                .Where(i => i.InventoryAuditId == audit.Id && i.CountedStock.HasValue)
                .ToListAsync();

            audit.TotalVariance = items.Sum(i => i.Variance ?? 0);
            audit.TotalVarianceValue = items.Sum(i => i.VarianceValue ?? 0);
            audit.UpdatedAt = DateTime.UtcNow;
        }

        private async Task<InventoryAuditDto> MapToDto(InventoryAudit audit)
        {
            return new InventoryAuditDto
            {
                Id = audit.Id,
                OrganizationId = audit.OrganizationId,
                Name = audit.Name,
                Description = audit.Description,
                Status = audit.Status.ToString(),
                SnapshotTakenAt = audit.SnapshotTakenAt,
                StartedAt = audit.StartedAt,
                CompletedAt = audit.CompletedAt,
                CreatedByUserId = audit.CreatedByUserId,
                CreatedByUserName = audit.CreatedByUser?.FirstName + " " + audit.CreatedByUser?.LastName,
                CompletedByUserId = audit.CompletedByUserId,
                CompletedByUserName = audit.CompletedByUser != null
                    ? audit.CompletedByUser.FirstName + " " + audit.CompletedByUser.LastName
                    : null,
                TotalVariants = audit.TotalVariants,
                CountedVariants = audit.CountedVariants,
                TotalVariance = audit.TotalVariance,
                TotalVarianceValue = audit.TotalVarianceValue,
                Notes = audit.Notes,
                CreatedAt = audit.CreatedAt,
                UpdatedAt = audit.UpdatedAt,
                // Scope fields
                ScopeType = audit.ScopeType ?? "Total",
                LocationId = audit.LocationId,
                LocationName = audit.Location?.Name,
                ScopeDescription = audit.ScopeDescription
            };
        }

        private InventoryAuditItemDto MapItemToDto(InventoryAuditItem item)
        {
            return new InventoryAuditItemDto
            {
                Id = item.Id,
                InventoryAuditId = item.InventoryAuditId,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductVariant?.Product?.Name ?? "",
                VariantSku = item.ProductVariant?.Sku ?? "",
                VariantSize = item.ProductVariant?.Size,
                VariantColor = item.ProductVariant?.Color,
                VariantImageUrl = item.ProductVariant?.ImageUrl,
                TheoreticalStock = item.TheoreticalStock,
                CountedStock = item.CountedStock,
                Variance = item.Variance,
                VarianceValue = item.VarianceValue,
                SnapshotAverageCost = item.SnapshotAverageCost,
                CountedByUserId = item.CountedByUserId,
                CountedByUserName = item.CountedByUser != null
                    ? item.CountedByUser.FirstName + " " + item.CountedByUser.LastName
                    : null,
                CountedAt = item.CountedAt,
                AdjustmentMovementId = item.AdjustmentMovementId,
                Notes = item.Notes
            };
        }

        #endregion
    }
}
