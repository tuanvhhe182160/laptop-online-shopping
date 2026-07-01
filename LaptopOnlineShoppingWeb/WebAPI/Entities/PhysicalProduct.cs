using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

[Table("PhysicalProducts")]
public partial class PhysicalProduct
{
    [Key]
    public int PhysicalId { get; set; }

    public int VariantId { get; set; }
    public int BranchId { get; set; }
    public int? OrderId { get; set; }

    [StringLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = "InStock";

    // Navigation properties (Tùy chọn, thêm JsonIgnore nếu cần)
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Branch Branch { get; set; } = null!;
    public virtual Order? Order { get; set; }
}
