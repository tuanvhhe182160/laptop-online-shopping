using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Categories
{
    public class CategoryRequestDTO
    {
        [Required(ErrorMessage = "Tên hãng không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên hãng không quá 100 ký tự.")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Mô tả không quá 255 ký tự.")]
        public string? Description { get; set; }
    }
}
