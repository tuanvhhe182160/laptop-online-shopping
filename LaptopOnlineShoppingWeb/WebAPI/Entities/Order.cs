namespace WebAPI.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public int? BranchId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public bool? PaymentStatus { get; set; }

    public string? OrderStatus { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<PhysicalProduct> PhysicalProducts { get; set; } = new List<PhysicalProduct>();
}
