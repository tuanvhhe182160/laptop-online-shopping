using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Enums;
using WebAPI.Repositories;

namespace WebAPI.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDTO?> CheckoutFromCartAsync(int customerId, CheckoutFromCartRequestDTO dto);
        Task<OrderResponseDTO?> CheckoutDirectlyAsync(int customerId, DirectCheckoutRequestDTO dto);
        Task<IEnumerable<OrderResponseDTO>> GetOrdersByCustomerAsync(int customerId);
        Task<OrderWithDetailsResponseDTO?> GetOrderDetailsAsync(int orderId);
        Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDTO dto);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ApplicationDbContext _context;

        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _context = context;
        }

        public async Task<OrderResponseDTO?> CheckoutFromCartAsync(int customerId, CheckoutFromCartRequestDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy giỏ hàng kèm theo thông tin Variant và Giá tiền
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null || !cart.CartItems.Any()) return null;

                // Tính tổng tiền dựa trên giá của Variant
                decimal totalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.ProductVariant.Price);

                // 1. Tạo đơn hàng cơ sở trước để DB cấp phát OrderId
                var order = new Order
                {
                    CustomerId = customerId,
                    BranchId = 1, // Tạm gán cho Chi nhánh tổng (hoặc có thể truyền từ dto.BranchId)
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                    PaymentStatus = false,
                    OrderStatus = OrderStatus.Pending
                };

                await _orderRepository.AddAsync(order);
                await _context.SaveChangesAsync(); // Lấy OrderId

                var orderDetails = new List<OrderDetail>();

                // 2. Quét giỏ hàng và xử lý Race Condition (Trừ kho vật lý)
                foreach (var item in cart.CartItems)
                {
                    // Lệnh SQL Nguyên tử: Khóa và cập nhật chính xác số lượng máy đang InStock
                    var updateSql = @"
                        UPDATE TOP (@p0) PhysicalProducts 
                        SET Status = 'Sold', OrderId = @p1 
                        WHERE VariantId = @p2 AND Status = 'InStock'";

                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        updateSql,
                        item.Quantity,    // Số lượng khách mua
                        order.OrderId,    // Mã đơn vừa sinh ra
                        item.VariantId    // Cấu hình máy
                    );

                    // Nếu số máy update thành công ít hơn số lượng khách cần -> Hết hàng
                    if (rowsAffected < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return null; // Khách hàng chậm tay đã mất phần, hủy toàn bộ giao dịch
                    }

                    // Lưu chi tiết lịch sử giá
                    orderDetails.Add(new OrderDetail
                    {
                        OrderId = order.OrderId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.ProductVariant.Price
                    });
                }

                await _context.OrderDetails.AddRangeAsync(orderDetails);

                // Dọn sạch giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod.ToString(),
                    PaymentStatus = order.PaymentStatus,
                    OrderStatus = order.OrderStatus.ToString(),
                    BranchId = order.BranchId
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderResponseDTO?> CheckoutDirectlyAsync(int customerId, DirectCheckoutRequestDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lưu ý: DTO của bạn cần đổi VariantId thành VariantId
                var variant = await _context.ProductVariants.FindAsync(dto.VariantId);
                if (variant == null) return null;

                decimal totalAmount = variant.Price * dto.Quantity;

                var order = new Order
                {
                    CustomerId = customerId,
                    BranchId = 1, // Mặc định hoặc lấy từ DTO
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                    PaymentStatus = false,
                    OrderStatus = OrderStatus.Pending
                };

                await _orderRepository.AddAsync(order);
                await _context.SaveChangesAsync();

                // Xử lý Race Condition cho đơn mua trực tiếp
                var updateSql = @"
                    UPDATE TOP (@p0) PhysicalProducts 
                    SET Status = 'Sold', OrderId = @p1 
                    WHERE VariantId = @p2 AND Status = 'InStock'";

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    updateSql, dto.Quantity, order.OrderId, dto.VariantId);

                if (rowsAffected < dto.Quantity)
                {
                    await transaction.RollbackAsync();
                    return null; // Hết hàng vật lý
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    VariantId = dto.VariantId,
                    Quantity = dto.Quantity,
                    UnitPrice = variant.Price
                };

                await _context.OrderDetails.AddAsync(orderDetail);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderResponseDTO
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod.ToString(),
                    PaymentStatus = order.PaymentStatus,
                    OrderStatus = order.OrderStatus.ToString(),
                    BranchId = order.BranchId
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _orderRepository.GetOrdersByCustomerIdAsync(customerId);
            return orders.Select(o => new OrderResponseDTO
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod.ToString(),
                PaymentStatus = o.PaymentStatus,
                OrderStatus = o.OrderStatus.ToString(),
                BranchId = o.BranchId
            });
        }

        public async Task<OrderWithDetailsResponseDTO?> GetOrderDetailsAsync(int orderId)
        {
            // Bổ sung Include để truy xuất tới tận tên sản phẩm gốc (Product) qua Variant
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return null;

            var physicalProducts = await _context.PhysicalProducts
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            return new OrderWithDetailsResponseDTO
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? "",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod.ToString(),
                PaymentStatus = order.PaymentStatus,
                OrderStatus = order.OrderStatus.ToString(),
                BranchId = order.BranchId,
                Details = order.OrderDetails.Select(od => new OrderDetailResponseDTO
                {
                    VariantId = od.VariantId, // Đổi từ VariantId sang VariantId
                    LaptopName = od.ProductVariant.Product.ProductName + $" ({od.ProductVariant.RAM} - {od.ProductVariant.SSD})",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    SerialNumbers = physicalProducts
                        .Where(p => p.VariantId == od.VariantId)
                        .Select(p => p.SerialNumber)
                        .ToList()
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders.Include(o => o.Customer).OrderByDescending(o => o.OrderDate).ToListAsync();
            return orders.Select(o => new OrderResponseDTO
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.FullName ?? "",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                PaymentMethod = o.PaymentMethod.ToString(),
                PaymentStatus = o.PaymentStatus,
                OrderStatus = o.OrderStatus.ToString(),
                BranchId = o.BranchId
            });
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatusUpdateDTO dto)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            if (Enum.TryParse<OrderStatus>(dto.OrderStatus, out var status))
            {
                order.OrderStatus = status;
            }
            order.PaymentStatus = dto.PaymentStatus;

            _orderRepository.Update(order);
            await _orderRepository.SaveAsync();
            return true;
        }
    }
}