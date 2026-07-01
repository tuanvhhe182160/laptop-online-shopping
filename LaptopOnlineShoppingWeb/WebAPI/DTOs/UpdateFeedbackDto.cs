using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class UpdateFeedbackDto
    {
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
