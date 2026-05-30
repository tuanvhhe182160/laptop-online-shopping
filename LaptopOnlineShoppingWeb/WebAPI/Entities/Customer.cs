using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("Customers")]
public partial class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(255)]
    [JsonIgnore]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string Email { get; set; } = null!;

    [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ.")]
    [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
    public string? Phone { get; set; }

    [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    [JsonIgnore]
    public virtual Cart? Cart { get; set; }

    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
