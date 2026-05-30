using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("CartItems")]
[PrimaryKey(nameof(CartId), nameof(LaptopId))]
public partial class CartItem
{
    [ForeignKey("Cart")]
    public int CartId { get; set; }

    [ForeignKey("Laptop")]
    public int LaptopId { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0.")]
    public int Quantity { get; set; }

    [JsonIgnore]
    public virtual Cart Cart { get; set; } = null!;

    [JsonIgnore]
    public virtual Laptop Laptop { get; set; } = null!;
}
