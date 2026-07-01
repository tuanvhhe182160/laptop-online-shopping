using WebAPI.Enums;

namespace WebClient.Models
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public bool? PaymentStatus { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public int? BranchId { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int VariantId { get; set; }
        public string LaptopName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }

    public class OrderWithDetailsViewModel : OrderViewModel
    {
        public List<OrderDetailViewModel> Details { get; set; } = new List<OrderDetailViewModel>();
    }
}
