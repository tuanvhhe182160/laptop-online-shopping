using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.DTOs;

namespace WebAPI.Services
{
    public interface IProductService
    {
        // Product
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<bool> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);

        // Product Variant
        Task<IEnumerable<ProductVariantDto>> GetVariantsByProductIdAsync(int productId);
        Task<ProductVariantDto?> GetVariantByIdAsync(int variantId);
        Task<ProductVariantDto> CreateVariantAsync(CreateProductVariantDto dto);
        Task<bool> UpdateVariantAsync(int id, UpdateProductVariantDto dto);
        Task<bool> DeleteVariantAsync(int id);
    }
}
