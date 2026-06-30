namespace WebAPI.DTOs
{
    public class CheckoutRequestDTO
    {
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        // Có thể thêm các field khác như Ghi chú, Mã giảm giá...
    }
}