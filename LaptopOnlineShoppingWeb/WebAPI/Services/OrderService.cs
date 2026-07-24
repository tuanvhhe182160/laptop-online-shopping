using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order?> GetOrderDetailsAsync(int orderId);
        Task<Order> CheckoutFromCartAsync(int customerId, CheckoutFromCartRequestDTO dto);
        Task<Order> CheckoutDirectlyAsync(int customerId, DirectCheckoutRequestDTO dto);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDTO dto);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDTO dto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.OrderStatus = dto.OrderStatus;
            order.PaymentStatus = dto.PaymentStatus;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order> CheckoutFromCartAsync(int customerId, CheckoutFromCartRequestDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || !cart.CartItems.Any())
                    throw new Exception("Giỏ hàng của bạn đang trống.");

                decimal totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.ProductVariant.Price);

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = false,
                    OrderStatus = "Pending",
                    BranchId = null
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart.CartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.ProductVariant.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Khóa dòng và cập nhật kho vật lý
                    string sql = @"
                        UPDATE TOP (@p0) PhysicalProducts
                        SET Status = 'Reserved', OrderId = @p1
                        WHERE VariantId = @p2 AND Status = 'InStock'";

                    int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        sql, item.Quantity, order.OrderId, item.VariantId);

                    if (rowsAffected < item.Quantity)
                        throw new Exception($"Sản phẩm (Mã loại: {item.VariantId}) chỉ còn {rowsAffected} máy. Vui lòng giảm số lượng.");
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> CheckoutDirectlyAsync(int customerId, DirectCheckoutRequestDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var variant = await _context.ProductVariants.FindAsync(dto.VariantId);
                if (variant == null) throw new Exception("Sản phẩm không tồn tại.");

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = variant.Price * dto.Quantity,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = false,
                    OrderStatus = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    VariantId = dto.VariantId,
                    Quantity = dto.Quantity,
                    UnitPrice = variant.Price
                };
                _context.OrderDetails.Add(orderDetail);

                string sql = @"
                    UPDATE TOP (@p0) PhysicalProducts
                    SET Status = 'Reserved', OrderId = @p1
                    WHERE VariantId = @p2 AND Status = 'InStock'";

                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    sql, dto.Quantity, order.OrderId, dto.VariantId);

                if (rowsAffected < dto.Quantity)
                    throw new Exception($"Sản phẩm này chỉ còn {rowsAffected} máy trong kho.");

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}