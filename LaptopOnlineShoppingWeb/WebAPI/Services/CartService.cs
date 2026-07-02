using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Services
{
    public interface ICartService
    {
        Task<CartResponseDTO?> GetCartAsync(int customerId);
        Task<bool> AddToCartAsync(int customerId, AddToCartRequestDTO dto);
        Task<bool> UpdateCartItemAsync(int customerId, UpdateCartItemDTO dto);
        Task<bool> RemoveFromCartAsync(int customerId, int laptopId);
        Task<bool> ClearCartAsync(int customerId);
    }

    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IGenericRepository<ProductVariant> _variantRepository;
        private readonly ApplicationDbContext _context;

        public CartService(ICartRepository cartRepository, IGenericRepository<ProductVariant> variantRepository, ApplicationDbContext context)
        {
            _cartRepository = cartRepository;
            _variantRepository = variantRepository;
            _context = context;
        }

        public async Task<CartResponseDTO?> GetCartAsync(int customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                return new CartResponseDTO
                {
                    CustomerId = customerId,
                    Items = new List<CartItemResponseDTO>(),
                    TotalAmount = 0
                };
            }

            var response = new CartResponseDTO
            {
                CartId = cart.CartId,
                CustomerId = cart.CustomerId,
                Items = cart.CartItems.Select(ci => new CartItemResponseDTO
                {
                    VariantId = ci.VariantId,
                    LaptopName = ci.ProductVariant.Product != null ? ci.ProductVariant.Product.ProductName : "Sản phẩm",
                    UnitPrice = ci.ProductVariant.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };
            response.TotalAmount = response.Items.Sum(i => i.TotalPrice);
            return response;
        }

        public async Task<bool> AddToCartAsync(int customerId, AddToCartRequestDTO dto)
        {
            var variant = await _variantRepository.GetByIdAsync(dto.VariantId);
            if (variant == null) return false;

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                await _cartRepository.AddAsync(cart);
                //lưu xuống DB ngay để EF Core sinh ra CartId tự động
                await _cartRepository.SaveAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == dto.VariantId);

            int newQuantity = dto.Quantity;
            if (existingItem != null)
            {
                newQuantity += existingItem.Quantity;
            }

            // Check Stock dựa vào số lượng PhysicalProduct InStock
            var inStockCount = await _context.PhysicalProducts
                .CountAsync(p => p.VariantId == dto.VariantId && p.Status == "InStock");
            
            if (newQuantity > inStockCount)
            {
                return false; // Vượt quá tồn kho
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.CartId,
                    VariantId = dto.VariantId,
                    Quantity = dto.Quantity
                });
            }

            await _cartRepository.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemAsync(int customerId, UpdateCartItemDTO dto)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null) return false;

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == dto.VariantId);
            if (existingItem == null) return false;

            var variant = await _variantRepository.GetByIdAsync(dto.VariantId);
            if (variant == null) return false;

            existingItem.Quantity = dto.Quantity;
            await _cartRepository.SaveAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(int customerId, int variantId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null) return false;

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == variantId);
            if (existingItem == null) return false;

            cart.CartItems.Remove(existingItem);
            await _cartRepository.SaveAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(int customerId)
        {
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            if (cart == null) return false;

            cart.CartItems.Clear();
            await _cartRepository.SaveAsync();
            return true;
        }
    }
}
