using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

[Table("Users")]
public partial class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

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

    [StringLength(255, ErrorMessage = "Đường dẫn ảnh đại diện không vượt quá 255 ký tự.")]
    public string? AvatarUrl { get; set; }

    [StringLength(100)]
    [JsonIgnore] 
    public string? ResetPasswordToken { get; set; }

    [Column(TypeName = "datetime")]
    [JsonIgnore] 
    public DateTime? ResetPasswordExpiry { get; set; }

    public bool? IsActive { get; set; }

    [Required(ErrorMessage = "Quyền (Role) là bắt buộc.")]
    [ForeignKey("Role")]
    public int RoleId { get; set; }

    [JsonIgnore] 
    public virtual Role Role { get; set; } = null!;
}
