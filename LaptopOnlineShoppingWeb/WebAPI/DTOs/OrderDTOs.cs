using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    // 1. DTO dùng cho API /api/orders/checkout-cart (Thanh toán toàn bộ Giỏ hàng)
    public class CheckoutFromCartRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = null!; // Ví dụ: "COD", "VNPAY", "ChuyenKhoan"

        // Các field mở rộng (Dù bảng Order của bạn hiện chưa có trường ShippingAddress, 
        // nhưng cứ thiết kế sẵn ở DTO để sau này update database dễ dàng hơn)
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
    }

    // 2. DTO dùng cho API /api/orders/checkout-direct (Tính năng "Mua Ngay" - bỏ qua giỏ hàng)
    public class DirectCheckoutRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm cần mua.")]
        public int VariantId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng mua phải lớn hơn 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = null!;

        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
    }

    // 3. DTO dùng cho API /api/orders/{id}/status (Dành cho Admin/Staff duyệt đơn)
    public class OrderStatusUpdateDTO
    {
        [Required(ErrorMessage = "Trạng thái đơn hàng không được để trống.")]
        public string OrderStatus { get; set; } = null!; // Ví dụ: "Pending", "Processing", "Shipped", "Cancelled"

        public bool PaymentStatus { get; set; } // true: Đã thanh toán, false: Chưa thanh toán
    }
}