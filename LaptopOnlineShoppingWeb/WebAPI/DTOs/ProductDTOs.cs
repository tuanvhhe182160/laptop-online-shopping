using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool? Status { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public List<ProductVariantDto> ProductVariants { get; set; } = new List<ProductVariantDto>();
    }

    public class ProductVariantDto
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string? Cpu { get; set; }
        public string? Ram { get; set; }
        public string? Ssd { get; set; }
        public string? Color { get; set; }
        public decimal Price { get; set; }

        // Added for Stock management logic later
        public int InStockCount { get; set; } 
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public string ProductCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }
        public bool? Status { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }
    }

    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public string ProductCode { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string ProductName { get; set; } = null!;

        public string? Description { get; set; }
        public bool? Status { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }
    }

    public class CreateProductVariantDto
    {
        [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
        public int ProductId { get; set; }

        public string? Cpu { get; set; }
        public string? Ram { get; set; }
        public string? Ssd { get; set; }
        public string? Color { get; set; }

        [Required(ErrorMessage = "Giá tiền là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0")]
        public decimal Price { get; set; }
    }

    public class UpdateProductVariantDto
    {
        public string? Cpu { get; set; }
        public string? Ram { get; set; }
        public string? Ssd { get; set; }
        public string? Color { get; set; }

        [Required(ErrorMessage = "Giá tiền là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 0")]
        public decimal Price { get; set; }
    }
}
