using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("CartItems")]
[PrimaryKey(nameof(CartId), nameof(VariantId))]
public partial class CartItem
{
    [ForeignKey("Cart")]
    public int CartId { get; set; }

    [ForeignKey("ProductVariant")]
    public int VariantId { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0.")]
    public int Quantity { get; set; }

    [JsonIgnore]
    public virtual Cart Cart { get; set; } = null!;

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}