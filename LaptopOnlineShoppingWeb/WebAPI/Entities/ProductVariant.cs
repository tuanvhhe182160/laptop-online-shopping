using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class ProductVariant
{
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    public string? Cpu { get; set; }

    public string? Ram { get; set; }

    public string? Ssd { get; set; }

    public string? Color { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<PhysicalProduct> PhysicalProducts { get; set; } = new List<PhysicalProduct>();

    public virtual Product Product { get; set; } = null!;
}
