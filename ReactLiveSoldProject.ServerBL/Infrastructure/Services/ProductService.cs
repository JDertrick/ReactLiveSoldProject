using Microsoft.EntityFrameworkCore;
using Mapster;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using System.Xml;

using ReactLiveSoldProject.ServerBL.DTOs.Common;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly LiveSoldDbContext _dbContext;

        public ProductService(LiveSoldDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<ProductDto>> GetProductsAsync(Guid organizationId, int page, int pageSize, string? status, string? searchTerm)
        {
            var query = _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Where(p => p.OrganizationId == organizationId)
                .ProjectToType<ProductDto>();

            if (!string.IsNullOrEmpty(status))
                if (status.Equals("published", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.IsPublished);
                else if (status.Equals("draft", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => !p.IsPublished);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.ToLower().Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(normalizedSearchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(normalizedSearchTerm)) ||
                    p.Variants.Any(v => v.Sku != null && v.Sku.ToLower().Contains(normalizedSearchTerm))
                );
            }

            var totalItems = await query.CountAsync();
            var productDtos = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var dto in productDtos)
            {
                if (dto.Variants.Any())
                {
                    var primaryVariant = dto.Variants.FirstOrDefault(v => v.IsPrimary);

                    dto.Sku = primaryVariant?.Sku ?? dto.Variants.FirstOrDefault()?.Sku ?? "";
                    dto.Stock = dto.Variants.Sum(v => v.StockQuantity);
                }
                else
                {
                    dto.Sku = "";
                    dto.Stock = 0;
                }
            }

            return new PagedResult<ProductDto>(productDtos, totalItems, page, pageSize);
        }

        public async Task<PagedResult<VariantProductDto>> GetVarantProductsAsync(Guid organizationId, int page, int pageSize, string? status, string? searchTerm)
        {
            try
            {
                // Obtener variantes
                var variantsQuery = BuildVariantsQuery(organizationId, status, searchTerm);
                var variantDtos = await variantsQuery.OrderByDescending(pv => pv.CreatedAt).ToListAsync();

                // Obtener productos sin variantes
                var productsQuery = BuildProductsWithoutVariantsQuery(organizationId, status, searchTerm);
                var productsWithoutVariants = await productsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();

                // // Convertir a DTOs y combinar
                // var variantDtos = variants.Select(MapVariantToDto).ToList();
                var productDtos = productsWithoutVariants.Select(MapProductToDto).ToList();
                var combinedResults = variantDtos.Concat(productDtos).OrderByDescending(r => r.CreatedAt).ToList();

                // Paginar
                var totalItems = combinedResults.Count;
                var pagedResults = combinedResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Cargar Product completo para items paginados
                await LoadFullProductDetails(pagedResults);

                return new PagedResult<VariantProductDto>(pagedResults, totalItems, page, pageSize);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IQueryable<VariantProductDto> BuildVariantsQuery(Guid organizationId, string? status, string? searchTerm)
        {
            var query = _dbContext.ProductVariants
                .Include(pv => pv.Product)
                .Where(pv => pv.OrganizationId == organizationId)
                .ProjectToType<VariantProductDto>();

            if (!string.IsNullOrEmpty(status) && string.Compare(status, "all", StringComparison.OrdinalIgnoreCase) != 0)
            {
                var isPublished = status.Equals("published", StringComparison.OrdinalIgnoreCase);
                query = query.Where(pv => pv.Product.IsPublished == isPublished);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower().Trim();
                query = query.Where(pv =>
                    pv.Product.Name.ToLower().Contains(term) ||
                    (pv.Product.Description != null && pv.Product.Description.ToLower().Contains(term)) ||
                    (pv.Sku != null && pv.Sku.ToLower().Contains(term))
                );
            }

            return query;
        }

        private IQueryable<Product> BuildProductsWithoutVariantsQuery(Guid organizationId, string? status, string? searchTerm)
        {
            var query = _dbContext.Products
                .Include(p => p.Variants)
                .Where(p => p.OrganizationId == organizationId && !p.Variants.Any());

            if (!string.IsNullOrEmpty(status))
            {
                var isPublished = status.Equals("published", StringComparison.OrdinalIgnoreCase);
                query = query.Where(p => p.IsPublished == isPublished);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower().Trim();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term))
                );
            }

            return query;
        }

        private VariantProductDto MapProductToDto(Product product)
        {
            return new VariantProductDto
            {
                Id = Guid.Empty,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductDescription = product.Description,
                Sku = "",
                Price = product.BasePrice,
                StockQuantity = 0,
                Stock = 0,
                AverageCost = 0,
                ImageUrl = product.ImageUrl,
                IsPublished = product.IsPublished,
                ProductType = product.ProductType.ToString(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private async Task LoadFullProductDetails(List<VariantProductDto> results)
        {
            var productIds = results.Select(r => r.ProductId).Distinct().ToList();
            var products = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks).ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var dto in results)
            {
                var product = products.FirstOrDefault(p => p.Id == dto.ProductId);
                if (product != null)
                    dto.Product = product.Adapt<ProductDto>();
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId, Guid organizationId)
        {
            var product = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Where(p => p.Id == productId && p.OrganizationId == organizationId)
                .ProjectToType<ProductDto>()
                .FirstOrDefaultAsync();

            return product ?? new();
        }

        public async Task<List<ProductDto>> SearchProductsAsync(Guid organizationId, string searchTerm)
        {
            var normalizedSearch = searchTerm.ToLower().Trim();

            var products = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Where(p => p.OrganizationId == organizationId &&
                    (p.Name.ToLower().Contains(normalizedSearch) ||
                     p.Description != null && p.Description.ToLower().Contains(normalizedSearch)))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Adapt<List<ProductDto>>();
        }

        public async Task<ProductDto> CreateProductAsync(Guid organizationId, CreateProductDto dto)
        {
            try
            {
                // Validar ProductType
                if (!Enum.TryParse<ProductType>(dto.ProductType, out var productType))
                    throw new InvalidOperationException($"Tipo de producto inválido: {dto.ProductType}");

                // Crear el producto
                var productId = Guid.NewGuid();

                var product = dto.Adapt<Product>();
                product.Id = productId;
                product.OrganizationId = organizationId;
                product.ProductType = productType;
                product.CategoryId = dto.CategoryId;

                _dbContext.Products.Add(product);

                await _dbContext.SaveChangesAsync();

                // Recargar el producto con sus relaciones
                var createdProduct = await _dbContext.Products
                    .Include(p => p.Variants)
                    .Include(p => p.TagLinks)
                        .ThenInclude(pt => pt.Tag)
                    .FirstAsync(p => p.Id == productId);

                return createdProduct.Adapt<ProductDto>();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<ProductDto> UpdateProductAsync(Guid productId, Guid organizationId, UpdateProductDto dto)
        {
            try
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

                // Actualizar campos
                dto.Adapt(product);
                product.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                // Recargar con relaciones actualizadas
                await _dbContext.Entry(product).Collection(p => p.TagLinks).LoadAsync();
                foreach (var tagLink in product.TagLinks)
                {
                    await _dbContext.Entry(tagLink).Reference(pt => pt.Tag).LoadAsync();
                }

                await _dbContext.Entry(product).Collection(p => p.Variants).LoadAsync();

                return product.Adapt<ProductDto>();
            }
            catch (Exception ex)
            {

                throw ex;
            }
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
                .Include(p => p.Variants)
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

            // Si esta variante se marca como principal, quitar el flag de las demás
            if (dto.IsPrimary)
            {
                var currentPrimary = product.Variants.FirstOrDefault(v => v.IsPrimary);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                }
            }

            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ProductId = productId,
                Sku = dto.Sku,
                Price = dto.Price,
                WholesalePrice = dto.WholesalePrice,
                StockQuantity = dto.StockQuantity,
                Attributes = dto.Attributes,
                ImageUrl = dto.ImageUrl,
                IsPrimary = dto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Parsear attributes para Size y Color
            ParseVariantAttributes(variant);

            _dbContext.ProductVariants.Add(variant);

            // Si es la variante principal, actualizar la imagen del producto
            if (variant.IsPrimary && !string.IsNullOrEmpty(variant.ImageUrl))
            {
                product.ImageUrl = variant.ImageUrl;
            }

            await _dbContext.SaveChangesAsync();

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Sku = variant.Sku,
                Price = variant.Price,
                WholesalePrice = variant.WholesalePrice,
                StockQuantity = variant.StockQuantity,
                AverageCost = variant.AverageCost,
                Attributes = variant.Attributes,
                ImageUrl = variant.ImageUrl,
                IsPrimary = variant.IsPrimary,
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };
        }

        public async Task<ProductVariantDto> UpdateVariantAsync(Guid variantId, Guid organizationId, CreateProductVariantDto dto)
        {
            var variant = await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                    .ThenInclude(p => p.Variants)
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

            // Si esta variante se marca como principal, quitar el flag de las demás
            if (dto.IsPrimary && !variant.IsPrimary)
            {
                var currentPrimary = variant.Product.Variants.FirstOrDefault(v => v.IsPrimary && v.Id != variantId);
                if (currentPrimary != null)
                {
                    currentPrimary.IsPrimary = false;
                }
            }

            variant.Sku = dto.Sku;
            variant.Price = dto.Price;
            variant.WholesalePrice = dto.WholesalePrice;
            variant.StockQuantity = dto.StockQuantity;
            variant.Attributes = dto.Attributes;
            variant.ImageUrl = dto.ImageUrl;
            variant.IsPrimary = dto.IsPrimary;
            variant.UpdatedAt = DateTime.UtcNow;

            // Parsear attributes para Size y Color
            ParseVariantAttributes(variant);

            // Si es la variante principal, actualizar la imagen del producto
            if (variant.IsPrimary && !string.IsNullOrEmpty(variant.ImageUrl))
            {
                variant.Product.ImageUrl = variant.ImageUrl;
            }
            // Si dejó de ser la variante principal, limpiar la imagen del producto si era la misma
            else if (!variant.IsPrimary && variant.Product.ImageUrl == variant.ImageUrl)
            {
                // Buscar si hay otra variante principal
                var newPrimary = variant.Product.Variants.FirstOrDefault(v => v.IsPrimary && v.Id != variantId);
                variant.Product.ImageUrl = newPrimary?.ImageUrl;
            }

            await _dbContext.SaveChangesAsync();

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Sku = variant.Sku,
                Price = variant.Price,
                WholesalePrice = variant.WholesalePrice,
                StockQuantity = variant.StockQuantity,
                AverageCost = variant.AverageCost,
                Attributes = variant.Attributes,
                ImageUrl = variant.ImageUrl,
                IsPrimary = variant.IsPrimary,
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };
        }

        public async Task<ProductVariantDto> UpdateVariantImageAsync(Guid variantId, Guid organizationId, string imageUrl)
        {
            var variant = await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                    .ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(pv => pv.Id == variantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante no encontrada");

            variant.ImageUrl = imageUrl;
            variant.UpdatedAt = DateTime.UtcNow;

            // Si es la variante principal, actualizar la imagen del producto
            if (variant.IsPrimary)
            {
                variant.Product.ImageUrl = imageUrl;
            }

            await _dbContext.SaveChangesAsync();

            return variant.Adapt<ProductVariantDto>();
        }

        public async Task DeleteVariantAsync(Guid variantId, Guid organizationId)
        {
            var variant = await _dbContext.ProductVariants
                .Include(pv => pv.Product)
                    .ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(pv => pv.Id == variantId && pv.OrganizationId == organizationId);

            if (variant == null)
                throw new KeyNotFoundException("Variante no encontrada");

            // Verificar si tiene items en órdenes
            var hasOrderItems = await _dbContext.SalesOrderItems
                .AnyAsync(oi => oi.ProductVariantId == variantId);

            if (hasOrderItems)
            {
                throw new InvalidOperationException(
                    "No se puede eliminar la variante porque tiene items en órdenes de venta.");
            }

            // Si era la variante principal, asignar otra como principal o limpiar la imagen del producto
            if (variant.IsPrimary)
            {
                var otherVariant = variant.Product.Variants.FirstOrDefault(v => v.Id != variantId);
                if (otherVariant != null)
                {
                    otherVariant.IsPrimary = true;
                    variant.Product.ImageUrl = otherVariant.ImageUrl;
                }
                else
                {
                    variant.Product.ImageUrl = null;
                }
            }

            _dbContext.ProductVariants.Remove(variant);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene el stock por ubicación para una variante de producto
        /// </summary>
        public async Task<List<StockByLocationDto>> GetStockByLocationAsync(Guid variantId, Guid organizationId)
        {
            // Verificar que la variante pertenece a la organización
            var variant = await _dbContext.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.OrganizationId == organizationId);

            if (variant == null)
            {
                throw new KeyNotFoundException("La variante de producto no existe o no pertenece a su organización.");
            }

            // Obtener el stock agrupado por ubicación desde StockBatches
            var stockByLocation = await _dbContext.StockBatches
                .Where(sb => sb.ProductVariantId == variantId && sb.IsActive && sb.QuantityRemaining > 0)
                .GroupBy(sb => new { sb.LocationId, sb.Location.Name })
                .Select(g => new StockByLocationDto
                {
                    LocationId = g.Key.LocationId ?? Guid.Empty,
                    LocationName = g.Key.Name ?? "Sin ubicación",
                    Quantity = g.Sum(sb => sb.QuantityRemaining)
                })
                .OrderByDescending(s => s.Quantity)
                .ToListAsync();

            return stockByLocation;
        }

        /// <summary>
        /// Helper method para parsear attributes JSON y asignar Size y Color
        /// </summary>
        private static void ParseVariantAttributes(ProductVariant variant)
        {
            if (!string.IsNullOrEmpty(variant.Attributes))
            {
                try
                {
                    var attrs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(variant.Attributes);
                    if (attrs != null)
                    {
                        variant.Size = attrs.ContainsKey("size") ? attrs["size"] : null;
                        variant.Color = attrs.ContainsKey("color") ? attrs["color"] : null;
                    }
                }
                catch
                {
                    // Si falla el parsing, continuar sin size/color
                }
            }
        }
    }
}
