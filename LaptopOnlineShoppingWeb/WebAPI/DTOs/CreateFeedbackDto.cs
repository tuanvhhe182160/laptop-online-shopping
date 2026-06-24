using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class CreateFeedbackDto
    {
        [Required(ErrorMessage = "Mã cấu hình (VariantId) là bắt buộc.")]
        public int VariantId { get; set; }

        [Required(ErrorMessage = "Số điểm đánh giá là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Số sao đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống.")]
        public string Comment { get; set; } = null!;
    }
}
