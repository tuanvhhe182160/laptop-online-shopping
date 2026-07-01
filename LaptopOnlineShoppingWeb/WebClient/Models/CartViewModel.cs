using System.Collections.Generic;

namespace WebClient.Models
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public int CustomerId { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
    }

    public class CartItemViewModel
    {
        public int VariantId { get; set; }
        public string LaptopName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
