namespace WebAPI.Enums
{
    public enum OrderStatus
    {
        Pending,    // Đang chờ xử lý
        Processing, // Đang chuẩn bị hàng
        Shipped,    // Đang giao hàng
        Delivered,  // Đã giao thành công
        Cancelled   // Đã hủy
    }
}
