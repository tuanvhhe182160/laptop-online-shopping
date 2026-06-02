using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Laptops
{
    public class LaptopUpdateDto
    {
        [Required(ErrorMessage = "Mã laptop là bắt buộc.")]
        [StringLength(20, ErrorMessage = "Mã laptop không được vượt quá 20 ký tự.")]
        public string LaptopCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên laptop là bắt buộc.")]
        [StringLength(150, ErrorMessage = "Tên laptop không được vượt quá 150 ký tự.")]
        public string LaptopName { get; set; } = null!;

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn 0.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        public int CategoryId { get; set; }

        public bool Status { get; set; }
    }
}
