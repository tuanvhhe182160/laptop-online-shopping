using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebAPI.Enums;

namespace WebAPI.Entities;

[Table("Orders")]
public partial class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }

    [Required(ErrorMessage = "Mã khách hàng là bắt buộc.")]
    [ForeignKey("Customer")]
    public int CustomerId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Required(ErrorMessage = "Tổng tiền là bắt buộc.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Tổng tiền không được là số âm.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Phương thức thanh toán không được vượt quá 50 ký tự.")]
    public PaymentMethod PaymentMethod { get; set; }

    public bool? PaymentStatus { get; set; }

    [StringLength(50, ErrorMessage = "Trạng thái đơn hàng không được vượt quá 50 ký tự.")]
    public OrderStatus OrderStatus { get; set; }

    [JsonIgnore]
    public virtual Customer Customer { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
