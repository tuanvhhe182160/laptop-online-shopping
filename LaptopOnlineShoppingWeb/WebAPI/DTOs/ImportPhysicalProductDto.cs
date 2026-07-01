using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class ImportPhysicalProductDto
    {
        [Required(ErrorMessage = "Mã biến thể (VariantId) là bắt buộc.")]
        public int VariantId { get; set; }

        [Required(ErrorMessage = "Danh sách Serial Numbers là bắt buộc.")]
        [MinLength(1, ErrorMessage = "Cần ít nhất 1 Serial Number để nhập kho.")]
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }
}
