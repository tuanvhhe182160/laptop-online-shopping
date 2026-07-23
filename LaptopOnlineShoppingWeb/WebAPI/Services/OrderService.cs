using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs; // Thay bằng namespace chứa DTO của bạn
using WebAPI.Entities;

namespace WebAPI.Services
{
    public interface IOrderService
    {
        Task<Order> CheckoutFromCartAsync(int customerId, CheckoutFromCartRequestDTO dto);
        // Bạn có thể khai báo thêm các hàm GetAll, GetMyOrders... tại đây
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CheckoutFromCartAsync(int customerId, CheckoutFromCartRequestDTO dto)
        {
            // 1. MỞ DATABASE TRANSACTION
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Lấy dữ liệu Giỏ hàng của khách hàng hiện tại
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || !cart.CartItems.Any())
                {
                    throw new Exception("Giỏ hàng của bạn đang trống.");
                }

                // 3. Tính tổng tiền từ đơn giá hiện tại của ProductVariant
                decimal totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.ProductVariant.Price);

                // 4. Khởi tạo Đơn hàng (Order)
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = false,
                    OrderStatus = "Pending",
                    BranchId = null // Lúc mới đặt, chưa gán cho chi nhánh nào. Staff sẽ tự vào nhận.
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Ép lưu để hệ thống sinh ra OrderId

                // 5. VÒNG LẶP XỬ LÝ TRỪ KHO VÀ CHỐNG RACE CONDITION
                foreach (var item in cart.CartItems)
                {
                    // A. Tạo chi tiết đơn hàng (Lưu cứng UnitPrice)
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.ProductVariant.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // B. TRUY VẤN RAW SQL (LINH HỒN CỦA BÀI TOÁN)
                    // Lệnh UPDATE TOP (@Quantity) sẽ chộp lấy đúng số lượng máy InStock và gán luôn OrderId.
                    string sql = @"
                        UPDATE TOP (@p0) PhysicalProducts
                        SET Status = 'Reserved', OrderId = @p1
                        WHERE VariantId = @p2 AND Status = 'InStock'";

                    int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        sql,
                        item.Quantity,
                        order.OrderId,
                        item.VariantId
                    );

                    // C. Kiểm tra tồn kho vật lý thực tế
                    if (rowsAffected < item.Quantity)
                    {
                        // Nếu số dòng Update thành công ít hơn số khách mua -> Chứng tỏ trong kho vật lý bị thiếu máy
                        throw new Exception($"Rất tiếc! Sản phẩm (Mã loại: {item.VariantId}) chỉ còn {rowsAffected} máy trong kho. Vui lòng giảm số lượng.");
                    }
                }

                // 6. Dọn dẹp Giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems);

                // Lưu nốt OrderDetails và lệnh xóa CartItems
                await _context.SaveChangesAsync();

                // 7. HOÀN TẤT TRANSACTION
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                // NẾU CÓ BẤT KỲ LỖI NÀO (Hết hàng, Lỗi DB, Crash mạng...), ROLLBACK MỌI THỨ VỀ SỐ 0
                await transaction.RollbackAsync();
                throw; // Đẩy lỗi ra để Controller bắt được và báo về Frontend
            }
        }
    }
}