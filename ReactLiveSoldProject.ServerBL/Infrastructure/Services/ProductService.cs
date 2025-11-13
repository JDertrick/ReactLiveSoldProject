using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly LiveSoldDbContext _dbContext;

        public ProductService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProductDto>> GetProductsByOrganizationAsync(Guid organizationId, bool includeUnpublished = false)
        {
            var query = _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.OrganizationId == organizationId);

            if (!includeUnpublished)
                query = query.Where(p => p.IsPublished);

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(p => MapToDto(p)).ToList();
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId, Guid organizationId)
        {
            var product = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == productId && p.OrganizationId == organizationId);

            return product != null ? MapToDto(product) : null;
        }

        public async Task<List<ProductDto>> SearchProductsAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var products = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.OrganizationId == organizationId &&
                    (p.Name.ToLower().Contains(normalizedSearch) ||
                     p.Description != null && p.Description.ToLower().Contains(normalizedSearch)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(p => MapToDto(p)).ToList();
        }

        public async Task<ProductDto> CreateProductAsync(Guid organizationId, CreateProductDto dto)
        {
            try
            {
                // Validar ProductType
                if (!Enum.TryParse<ProductType>(dto.ProductType, out var productType))
                    throw new InvalidOperationException($"Tipo de producto inválido: {dto.ProductType}");

                // Verificar que los tags existan
                if (dto.TagIds.Any())
                {
                    var existingTagIds = await _dbContext.Tags
                        .Where(t => t.OrganizationId == organizationId && dto.TagIds.Contains(t.Id))
                        .Select(t => t.Id)
                        .ToListAsync();

                    var invalidTagIds = dto.TagIds.Except(existingTagIds).ToList();
                    if (invalidTagIds.Any())
                        throw new InvalidOperationException($"Tags no encontrados: {string.Join(", ", invalidTagIds)}");
                }

                // Validar que haya al menos una variante
                if (!dto.Variants.Any())
                    throw new InvalidOperationException("El producto debe tener al menos una variante");

                // Crear el producto
                var productId = Guid.NewGuid();
                var product = new Product
                {
                    Id = productId,
                    OrganizationId = organizationId,
                    Name = dto.Name,
                    Description = dto.Description,
                    ProductType = productType,
                    IsPublished = dto.IsPublished,
                    BasePrice = dto.BasePrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Products.Add(product);

                // Crear las variantes
                foreach (var variantDto in dto.Variants)
                {
                    var variant = new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organizationId,
                        ProductId = productId,
                        Sku = variantDto.Sku,
                        Price = variantDto.Price,
                        StockQuantity = variantDto.StockQuantity,
                        Attributes = variantDto.Attributes,
                        ImageUrl = variantDto.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _dbContext.ProductVariants.Add(variant);
                }

                // Asociar tags
                foreach (var tagId in dto.TagIds)
                {
                    var productTag = new ProductTag
                    {
                        ProductId = productId,
                        TagId = tagId
                    };

                    _dbContext.ProductTags.Add(productTag);
                }

                await _dbContext.SaveChangesAsync();

                // Recargar el producto con sus relaciones
                var createdProduct = await _dbContext.Products
                    .Include(p => p.Variants)
                    .Include(p => p.TagLinks)
                        .ThenInclude(pt => pt.Tag)
                    .FirstAsync(p => p.Id == productId);

                return MapToDto(createdProduct);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public async Task<ProductDto> UpdateProductAsync(Guid productId, Guid organizationId, UpdateProductDto dto)
        {
            var product = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == productId && p.OrganizationId == organizationId);

            if (product == null)
                throw new KeyNotFoundException("Producto no encontrado");

            // Validar ProductType
            if (!Enum.TryParse<ProductType>(dto.ProductType, out var productType))
                throw new InvalidOperationException($"Tipo de producto inválido: {dto.ProductType}");

            // Verificar que los tags existan
            if (dto.TagIds.Any())
            {
                var existingTagIds = await _dbContext.Tags
                    .Where(t => t.OrganizationId == organizationId && dto.TagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                var invalidTagIds = dto.TagIds.Except(existingTagIds).ToList();
                if (invalidTagIds.Any())
                    throw new InvalidOperationException($"Tags no encontrados: {string.Join(", ", invalidTagIds)}");
            }

            // Actualizar campos
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.ProductType = productType;
            product.BasePrice = dto.BasePrice;
            product.IsPublished = dto.IsPublished;
            product.UpdatedAt = DateTime.UtcNow;

            // Actualizar tags (eliminar existentes y agregar nuevos)
            var existingProductTags = await _dbContext.ProductTags
                .Where(pt => pt.ProductId == productId)
                .ToListAsync();

            _dbContext.ProductTags.RemoveRange(existingProductTags);

            foreach (var tagId in dto.TagIds)
            {
                var productTag = new ProductTag
                {
                    ProductId = productId,
                    TagId = tagId
                };

                _dbContext.ProductTags.Add(productTag);
            }

            await _dbContext.SaveChangesAsync();

            // Recargar con relaciones actualizadas
            await _dbContext.Entry(product).Collection(p => p.TagLinks).LoadAsync();
            foreach (var tagLink in product.TagLinks)
            {
                await _dbContext.Entry(tagLink).Reference(pt => pt.Tag).LoadAsync();
            }

            return MapToDto(product);
        }

        public async Task DeleteProductAsync(Guid productId, Guid organizationId)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.OrganizationId == organizationId);

            if (product == null)
                throw new KeyNotFoundException("Producto no encontrado");

            // Verificar si tiene items en órdenes
            var hasOrderItems = await _dbContext.SalesOrderItems
                .Include(oi => oi.ProductVariant)
                .AnyAsync(oi => oi.ProductVariant.ProductId == productId);

            if (hasOrderItems)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar el producto porque tiene items en órdenes de venta. " +
                    "Considere despublicarlo en lugar de eliminarlo.");
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<TagDto>> GetTagsByOrganizationAsync(Guid organizationId)
        {
            var tags = await _dbContext.Tags
                .Where(t => t.OrganizationId == organizationId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return tags.Select(t => new TagDto
            {
                Id = t.Id,
                OrganizationId = t.OrganizationId,
                Name = t.Name
            }).ToList();
        }

        public async Task<TagDto> CreateTagAsync(Guid organizationId, string name)
        {
            var normalizedName = name.Trim();

            // Verificar que no exista un tag con el mismo nombre
            var existingTag = await _dbContext.Tags
                .FirstOrDefaultAsync(t => t.OrganizationId == organizationId && t.Name == normalizedName);

            if (existingTag != null)
                throw new InvalidOperationException("Ya existe un tag con este nombre en la organización");

            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = normalizedName
            };

            _dbContext.Tags.Add(tag);
            await _dbContext.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                OrganizationId = tag.OrganizationId,
                Name = tag.Name
            };
        }

        public async Task<ProductVariantDto> AddVariantAsync(Guid productId, Guid organizationId, CreateProductVariantDto dto)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.OrganizationId == organizationId);

            if (product == null)
                throw new KeyNotFoundException("Producto no encontrado");

            // Verificar SKU único si se proporciona
            if (!string.IsNullOrWhiteSpace(dto.Sku))
            {
                var existingSku = await _dbContext.ProductVariants
                    .AnyAsync(pv => pv.OrganizationId == organizationId && pv.Sku == dto.Sku);

                if (existingSku)
                    throw new InvalidOperationException("Ya existe una variante con este SKU en la organización");
            }

            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductId = productId,
                Sku = dto.Sku,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Attributes = dto.Attributes,
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.ProductVariants.Add(variant);
            await _dbContext.SaveChangesAsync();

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Sku = variant.Sku,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                Attributes = variant.Attributes,
                ImageUrl = variant.ImageUrl,
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };
        }

        public async Task<ProductVariantDto> UpdateVariantAsync(Guid variantId, Guid organizationId, CreateProductVariantDto dto)
        {
            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(pv => pv.Id == variantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante no encontrada");

            // Verificar SKU único si cambió
            if (!string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != variant.Sku)
            {
                var existingSku = await _dbContext.ProductVariants
                    .AnyAsync(pv => pv.OrganizationId == organizationId && pv.Sku == dto.Sku && pv.Id != variantId);

                if (existingSku)
                    throw new InvalidOperationException("Ya existe otra variante con este SKU en la organización");
            }

            variant.Sku = dto.Sku;
            variant.Price = dto.Price;
            variant.StockQuantity = dto.StockQuantity;
            variant.Attributes = dto.Attributes;
            variant.ImageUrl = dto.ImageUrl;
            variant.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Sku = variant.Sku,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                Attributes = variant.Attributes,
                ImageUrl = variant.ImageUrl,
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };
        }

        public async Task DeleteVariantAsync(Guid variantId, Guid organizationId)
        {
            var variant = await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Id == variantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante no encontrada");

            // Verificar que no sea la última variante del producto
            var variantCount = await _dbContext.ProductVariants
                .CountAsync(pv => pv.ProductId == variant.ProductId);

            if (variantCount <= 1)
                throw new InvalidOperationException("No se puede eliminar la última variante del producto");

            // Verificar si tiene items en órdenes
            var hasOrderItems = await _dbContext.SalesOrderItems
                .AnyAsync(oi => oi.ProductVariantId == variantId);

            if (hasOrderItems)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar la variante porque tiene items en órdenes de venta.");
            }

            _dbContext.ProductVariants.Remove(variant);
            await _dbContext.SaveChangesAsync();
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                OrganizationId = product.OrganizationId,
                Name = product.Name,
                Description = product.Description,
                ProductType = product.ProductType.ToString(),
                IsPublished = product.IsPublished,
                BasePrice = product.BasePrice,
                Tags = product.TagLinks?.Select(pt => new TagDto
                {
                    Id = pt.Tag.Id,
                    OrganizationId = pt.Tag.OrganizationId,
                    Name = pt.Tag.Name
                }).ToList() ?? new List<TagDto>(),
                Variants = product.Variants?.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.Attributes,
                    ImageUrl = v.ImageUrl,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                }).ToList() ?? new List<ProductVariantDto>(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
