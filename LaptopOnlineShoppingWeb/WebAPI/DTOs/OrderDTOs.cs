using System;
using System.Collections.Generic;
using WebAPI.Enums;

namespace WebAPI.DTOs
{
    public class CheckoutFromCartRequestDTO
    {
        public string ShippingAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }

    public class DirectCheckoutRequestDTO
    {
        public int LaptopId { get; set; }
        public int Quantity { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }

    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public bool? PaymentStatus { get; set; }
        public string? OrderStatus { get; set; }
    }

    public class OrderDetailResponseDTO
    {
        public int LaptopId { get; set; }
        public string LaptopName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class OrderWithDetailsResponseDTO : OrderResponseDTO
    {
        public List<OrderDetailResponseDTO> Details { get; set; } = new List<OrderDetailResponseDTO>();
    }

    public class OrderStatusUpdateDTO
    {
        public string OrderStatus { get; set; } = null!;
        public bool PaymentStatus { get; set; }
    }
}
