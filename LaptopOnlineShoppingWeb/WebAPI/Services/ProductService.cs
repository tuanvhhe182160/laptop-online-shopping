using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;

        public ProductService(
            IProductRepository productRepository,
            IProductVariantRepository variantRepository)
        {
            _productRepository = productRepository;
            _variantRepository = variantRepository;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.PhysicalProducts) // to count stock
                .ToListAsync();

            return products.Select(p => MapToProductDto(p));
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetQueryable()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.PhysicalProducts)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            return MapToProductDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                ProductCode = dto.ProductCode,
                ProductName = dto.ProductName,
                Description = dto.Description,
                Status = dto.Status ?? true,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.Now
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            return MapToProductDto(product);
        }

        public async Task<bool> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            product.ProductCode = dto.ProductCode;
            product.ProductName = dto.ProductName;
            product.Description = dto.Description;
            product.Status = dto.Status ?? true;
            product.CategoryId = dto.CategoryId;

            _productRepository.Update(product);
            await _productRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetQueryable()
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.OrderDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return false;

            // Kiểm tra xem sản phẩm đã có ai mua chưa (nằm trong OrderDetails)
            bool isOrdered = product.ProductVariants.Any(v => v.OrderDetails.Any());

            if (isOrdered)
            {
                // Soft delete: Chuyển trạng thái sang Ngừng kinh doanh
                product.Status = false;
                _productRepository.Update(product);
            }
            else
            {
                // Hard delete: Xóa hoàn toàn nếu chưa ai mua
                _productRepository.Delete(product);
            }
            
            await _productRepository.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<ProductVariantDto>> GetVariantsByProductIdAsync(int productId)
        {
            var variants = await _variantRepository.GetQueryable()
                .Include(v => v.PhysicalProducts)
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            return variants.Select(v => MapToVariantDto(v));
        }

        public async Task<ProductVariantDto?> GetVariantByIdAsync(int variantId)
        {
            var variant = await _variantRepository.GetQueryable()
                .Include(v => v.PhysicalProducts)
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant == null) return null;

            return MapToVariantDto(variant);
        }

        public async Task<ProductVariantDto> CreateVariantAsync(CreateProductVariantDto dto)
        {
            var variant = new ProductVariant
            {
                ProductId = dto.ProductId,
                CPU = dto.CPU,
                RAM = dto.RAM,
                SSD = dto.SSD,
                Color = dto.Color,
                Price = dto.Price
            };

            await _variantRepository.AddAsync(variant);
            await _variantRepository.SaveAsync();

            return MapToVariantDto(variant);
        }

        public async Task<bool> UpdateVariantAsync(int id, UpdateProductVariantDto dto)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return false;

            variant.CPU = dto.CPU;
            variant.RAM = dto.RAM;
            variant.SSD = dto.SSD;
            variant.Color = dto.Color;
            variant.Price = dto.Price;

            _variantRepository.Update(variant);
            await _variantRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteVariantAsync(int id)
        {
            var variant = await _variantRepository.GetQueryable()
                .Include(v => v.OrderDetails)
                .FirstOrDefaultAsync(v => v.VariantId == id);

            if (variant == null) return false;

            // Chặn xóa cứng nếu đã có người mua
            if (variant.OrderDetails.Any()) return false;

            _variantRepository.Delete(variant);
            await _variantRepository.SaveAsync();
            return true;
        }

        private ProductDto MapToProductDto(Product product)
        {
            return new ProductDto
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Description = product.Description,
                CreatedDate = product.CreatedDate,
                Status = product.Status,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                ProductVariants = product.ProductVariants?.Select(v => MapToVariantDto(v)).ToList() ?? new List<ProductVariantDto>()
            };
        }

        private ProductVariantDto MapToVariantDto(ProductVariant variant)
        {
            return new ProductVariantDto
            {
                VariantId = variant.VariantId,
                ProductId = variant.ProductId,
                CPU = variant.CPU,
                RAM = variant.RAM,
                SSD = variant.SSD,
                Color = variant.Color,
                Price = variant.Price,
                InStockCount = variant.PhysicalProducts?.Count(p => p.Status == "InStock") ?? 0
            };
        }
    }
}
