using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs.Dashboard;
using WebAPI.Enums;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff,WarehouseManager")] 
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Tổng quan số liệu (Thẻ KPI)
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalBranches = await _context.Branches.CountAsync();

            // Chỉ tính doanh thu từ các đơn hàng đã hoàn thành
            var totalRevenue = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Delivered.ToString())
                .SumAsync(o => o.TotalAmount);

            return Ok(new DashboardSummaryDto
            {
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalBranches = totalBranches,
                TotalRevenue = totalRevenue
            });
        }

        // 2. Doanh thu theo tháng (Cho biểu đồ cột/đường)
        [HttpGet("revenue-by-month")]
        public async Task<IActionResult> GetRevenueByMonth()
        {
            var orders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Delivered.ToString())
                .ToListAsync();

            var revenueData = orders
                .GroupBy(o => new
                {
                    o.OrderDate!.Value.Year,
                    o.OrderDate.Value.Month
                })
                .Select(g => new RevenueByMonthDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            return Ok(revenueData);
        }

        // 3. Tỉ trọng doanh thu các chi nhánh
        [HttpGet("revenue-by-branch")]
        public async Task<IActionResult> GetRevenueByBranch()
        {
            var branchRevenue = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Delivered.ToString())
                .GroupBy(o => new
                {
                    o.BranchId,
                    BranchName = o.Branch!.BranchName
                })
                .Select(g => new RevenueByBranchDto
                {
                    // Nếu đơn online chưa gán branch thì ghi "Chưa phân phối"
                    BranchName = g.Key.BranchName ?? "Đơn Online chưa phân bổ",
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync();

            return Ok(branchRevenue);
        }

        // 4. Top 5 sản phẩm bán chạy (Dựa vào OrderDetails và bảng Products)
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts()
        {
            var topProducts = await _context.OrderDetails
                .Where(od => od.Order.OrderStatus == OrderStatus.Delivered.ToString())
                .GroupBy(od => od.ProductVariant.Product.ProductName)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    SoldQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.SoldQuantity)
                .Take(5)
                .ToListAsync();

            return Ok(topProducts);
        }
    }
}