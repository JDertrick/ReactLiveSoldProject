using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using AutoMapper.QueryableExtensions;
using System.Xml;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public ProductService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetProductsByOrganizationAsync(Guid organizationId, bool includeUnpublished = false)
        {
            var products = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Where(p => p.OrganizationId == organizationId)
                .OrderBy(p => p.Name)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // if (!includeUnpublished)
            //     query = query.Where(p => p.IsPublished);

            return products;
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId, Guid organizationId)
        {
            var product = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Category)
                .Where(p => p.Id == productId && p.OrganizationId == organizationId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
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

            return _mapper.Map<List<ProductDto>>(products);
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

                var product = _mapper.Map<Product>(dto);
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

                return _mapper.Map<ProductDto>(createdProduct);
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
                _mapper.Map(dto, product);
                product.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                // Recargar con relaciones actualizadas
                await _dbContext.Entry(product).Collection(p => p.TagLinks).LoadAsync();
                foreach (var tagLink in product.TagLinks)
                {
                    await _dbContext.Entry(tagLink).Reference(pt => pt.Tag).LoadAsync();
                }

                await _dbContext.Entry(product).Collection(p => p.Variants).LoadAsync();

                return _mapper.Map<ProductDto>(product);
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
                StockQuantity = variant.StockQuantity,
                AverageCost = variant.AverageCost,
                Attributes = variant.Attributes,
                ImageUrl = variant.ImageUrl,
                IsPrimary = variant.IsPrimary,
                CreatedAt = variant.CreatedAt,
                UpdatedAt = variant.UpdatedAt
            };
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
