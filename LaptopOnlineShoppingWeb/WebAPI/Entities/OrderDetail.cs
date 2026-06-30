using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("OrderDetails")]
[PrimaryKey(nameof(OrderId), nameof(VariantId))]
public partial class OrderDetail
{
    [ForeignKey("Order")]
    public int OrderId { get; set; }

    [ForeignKey("ProductVariant")]
    public int VariantId { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Đơn giá là bắt buộc.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Đơn giá không được là số âm.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    public virtual Order Order { get; set; } = null!;

    [JsonIgnore]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}