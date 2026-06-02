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
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
                if (cart == null || !cart.CartItems.Any())
                {
                    return null; // Giỏ hàng trống
                }

                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var item in cart.CartItems)
                {
                    var laptop = await _context.Laptops.FindAsync(item.LaptopId);
                    if (laptop == null || laptop.Status == false || laptop.StockQuantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return null; // Hết hàng hoặc không đủ
                    }

                    // Tính tiền và tạo chi tiết
                    totalAmount += laptop.Price * item.Quantity;
                    orderDetails.Add(new OrderDetail
                    {
                        LaptopId = laptop.LaptopId,
                        Quantity = item.Quantity,
                        UnitPrice = laptop.Price
                    });

                    // Trừ tồn kho
                    laptop.StockQuantity -= item.Quantity;
                    _context.Laptops.Update(laptop);
                }

                // Tạo đơn hàng
                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                    PaymentStatus = false,
                    OrderStatus = OrderStatus.Pending,
                    OrderDetails = orderDetails
                };

                await _orderRepository.AddAsync(order);

                // Dọn giỏ hàng
                cart.CartItems.Clear();

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
                    OrderStatus = order.OrderStatus.ToString()
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
                var laptop = await _context.Laptops.FindAsync(dto.LaptopId);
                if (laptop == null || laptop.Status == false || laptop.StockQuantity < dto.Quantity)
                {
                    return null; // Lỗi hết hàng
                }

                decimal totalAmount = laptop.Price * dto.Quantity;

                var orderDetail = new OrderDetail
                {
                    LaptopId = laptop.LaptopId,
                    Quantity = dto.Quantity,
                    UnitPrice = laptop.Price
                };

                laptop.StockQuantity -= dto.Quantity;
                _context.Laptops.Update(laptop);

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                    PaymentStatus = false,
                    OrderStatus = OrderStatus.Pending,
                    OrderDetails = new List<OrderDetail> { orderDetail }
                };

                await _orderRepository.AddAsync(order);
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
                    OrderStatus = order.OrderStatus.ToString()
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
                OrderStatus = o.OrderStatus.ToString()
            });
        }

        public async Task<OrderWithDetailsResponseDTO?> GetOrderDetailsAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
            if (order == null) return null;

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
                Details = order.OrderDetails.Select(od => new OrderDetailResponseDTO
                {
                    LaptopId = od.LaptopId,
                    LaptopName = od.Laptop.LaptopName,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
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
                OrderStatus = o.OrderStatus.ToString()
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
