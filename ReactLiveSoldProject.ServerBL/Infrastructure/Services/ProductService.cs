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

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid productId, Guid organizationId)
        {
            var product = await _dbContext.Products
                .Include(p => p.Variants)
                .Include(p => p.TagLinks)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == productId && p.OrganizationId == organizationId);

            return product != null ? _mapper.Map<ProductDto>(product) : null;
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

            return _mapper.Map<List<ProductDto>>(products);
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

                var variants = await _dbContext.Products.Where(p => p.OrganizationId == organizationId)
                    .SelectMany(pv => pv.Variants)
                    .ProjectTo<ProductVariantDto>(_mapper.ConfigurationProvider).ToListAsync();
                
                // pasa los skus a un array[string]
                var skusArray = variants.Select(v => v.Sku).ToHashSet();

                bool variantRepeat = dto.Variants.Any(v => skusArray.Contains(v.Sku));

                if (variantRepeat)
                    throw new InvalidOperationException("El producto contiene SKUs que ya existen en el inventario.");

                // Crear el producto
                var productId = Guid.NewGuid();
                
                var product = _mapper.Map<Product>(dto);
                product.Id = productId;
                product.OrganizationId = organizationId;
                product.ProductType = productType;

                _dbContext.Products.Add(product);

                // Crear las variantes usando AutoMapper
                foreach (var variantDto in dto.Variants)
                {
                    var variant = _mapper.Map<ProductVariant>(variantDto);
                    variant.Id = Guid.NewGuid();
                    variant.OrganizationId = organizationId;
                    variant.ProductId = productId;
                    variant.CreatedAt = DateTime.UtcNow;
                    variant.UpdatedAt = DateTime.UtcNow;

                    // Parsear attributes para Size y Color
                    ParseVariantAttributes(variant);

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
                product.ImageUrl = dto.ImageUrl;
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

                // Valida si existe algun SKU
                var dtoVariants = dto.Variants;
                //var variants = await _dbContext.Products.Where(p => p.OrganizationId == organizationId)
                //    .SelectMany(pv => pv.Variants)
                //    .ProjectTo<ProductVariantDto>(_mapper.ConfigurationProvider).ToListAsync();

                //// pasa los skus a un array[string]
                //var skusArray = variants.Select(v => v.Sku).ToHashSet();

                //bool variantRepeat = dtoVariants.Any(v => skusArray.Contains(v.Sku));

                //if (variantRepeat)
                //    throw new InvalidOperationException("El producto contiene SKUs que ya existen en el inventario.");

                // Actualizar variantes si se proporcionaron
                if (dto.Variants != null)
                {
                    var dbVariants = product.Variants;

                    var dtoVariantIds = dtoVariants
                        .Where(v => v.Id != Guid.Empty)
                        .Select(v => v.Id)
                        .ToHashSet();

                    var variantsToDelete = dbVariants
                        .Where(v => !dtoVariantIds.Contains(v.Id));

                    if (variantsToDelete.Any())
                    {
                        foreach(var variantToDelete in variantsToDelete)
                        {
                            var hasOrderItem = await _dbContext.ProductVariants
                                .Where(pv => pv.Id == variantToDelete.Id)
                                .AnyAsync(pv => pv.SalesOrderItems.Any());

                            if (hasOrderItem)
                            {
                                throw new InvalidOperationException("No se puede eliminar el producto porque tiene items en órdenes de venta.");
                            }
                        }
                        _dbContext.RemoveRange(variantsToDelete);
                    }

                    foreach (var variantDto in dtoVariants)
                    {
                        if (variantDto.Id == Guid.Empty)
                        {
                            var newVariant = _mapper.Map<ProductVariant>(variantDto);
                            newVariant.OrganizationId = organizationId;
                            newVariant.UpdatedAt = DateTime.UtcNow;
                            ParseVariantAttributes(newVariant);

                            product.Variants.Add(newVariant);
                        }
                        else
                        {
                            var existingVariant = dbVariants.FirstOrDefault(dbv => dbv.Id == variantDto.Id);

                            if (existingVariant != null)
                            {
                                _mapper.Map(variantDto, existingVariant);
                                existingVariant.UpdatedAt = DateTime.UtcNow;
                                ParseVariantAttributes(existingVariant);
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync();

                // Recargar con relaciones actualizadas
                await _dbContext.Entry(product).Collection(p => p.TagLinks).LoadAsync();
                foreach (var tagLink in product.TagLinks)
                {
                    await _dbContext.Entry(tagLink).Reference(pt => pt.Tag).LoadAsync();
                }

                // Recargar variantes si se actualizaron
                if (dto.Variants != null)
                {
                    await _dbContext.Entry(product).Collection(p => p.Variants).LoadAsync();
                }

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
