using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("Laptops")]
public partial class Laptop
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LaptopId { get; set; }

    [Required(ErrorMessage = "Mã laptop là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Mã laptop không được vượt quá 20 ký tự.")]
    public string LaptopCode { get; set; } = null!;

    [Required(ErrorMessage = "Tên laptop là bắt buộc.")]
    [StringLength(150, ErrorMessage = "Tên laptop không được vượt quá 150 ký tự.")]
    public string LaptopName { get; set; } = null!;

    [Required(ErrorMessage = "Giá sản phẩm là bắt buộc.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được là số âm.")]
    public int StockQuantity { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    public bool? Status { get; set; }

    [Required(ErrorMessage = "Danh mục là bắt buộc.")]
    [ForeignKey("Category")]
    public int CategoryId { get; set; }

    [JsonIgnore]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [JsonIgnore]
    public virtual Category Category { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
