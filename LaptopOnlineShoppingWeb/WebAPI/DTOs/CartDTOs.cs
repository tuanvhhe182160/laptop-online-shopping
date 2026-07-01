using System.Collections.Generic;

namespace WebAPI.DTOs
{
    public class AddToCartRequestDTO
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDTO
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemResponseDTO
    {
        public int VariantId { get; set; }
        public string LaptopName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class CartResponseDTO
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public List<CartItemResponseDTO> Items { get; set; } = new List<CartItemResponseDTO>();
        public decimal TotalAmount { get; set; }
    }
}
