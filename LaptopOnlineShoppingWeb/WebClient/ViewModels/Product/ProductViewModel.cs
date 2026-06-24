using System.Collections.Generic;

namespace WebClient.ViewModels.Product
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public bool? Status { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public List<ProductVariantViewModel> ProductVariants { get; set; } = new List<ProductVariantViewModel>();
    }

    public class ProductVariantViewModel
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? Cpu { get; set; }
        public string? Ram { get; set; }
        public string? Ssd { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }
        public int InStockCount { get; set; }
    }
}
