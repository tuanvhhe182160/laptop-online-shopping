using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("ProductVariants")]
public partial class ProductVariant
{
    [Key]
    public int VariantId { get; set; }
    public int ProductId { get; set; }
    public string? CPU { get; set; }
    public string? RAM { get; set; }
    public string? SSD { get; set; }
    public string? Color { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0")]
    public decimal Price { get; set; }

    [JsonIgnore]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [JsonIgnore]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
