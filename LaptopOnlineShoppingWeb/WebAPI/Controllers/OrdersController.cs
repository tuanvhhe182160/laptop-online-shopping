using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        private int GetCurrentCustomerId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdStr!);
        }

        [HttpGet]
        [EnableQuery] // Hỗ trợ OData ($filter, $orderby...)
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllOrdersAsync();

            // Phân quyền dữ liệu: Staff chỉ được xem đơn hàng của chi nhánh mình
            if (User.IsInRole("Staff"))
            {
                var branchIdClaim = User.FindFirst("BranchId")?.Value;
                if (!string.IsNullOrEmpty(branchIdClaim) && int.TryParse(branchIdClaim, out int branchId))
                {
                    // Lọc đơn hàng theo BranchId (Yêu cầu bổ sung thuộc tính BranchId vào OrderResponseDTO)
                    orders = orders.Where(o => o.BranchId == branchId);
                }
                else
                {
                    return Forbid("Tài khoản Staff của bạn chưa được gắn với Chi nhánh nào.");
                }
            }

            return Ok(orders.AsQueryable()); // Ép kiểu AsQueryable để OData hoạt động tối ưu
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            var orders = await _service.GetOrdersByCustomerAsync(GetCurrentCustomerId());
            return Ok(orders);
        }

        [HttpGet("{id}/details")]
        [Authorize] // Bắt buộc đăng nhập (Khách xem đơn của mình, Admin/Staff xem để duyệt)
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _service.GetOrderDetailsAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng.");

            // Nếu là Customer, chặn không cho xem đơn của người khác
            if (User.IsInRole("Customer") && order.CustomerId != GetCurrentCustomerId())
            {
                return Forbid("Bạn không có quyền xem đơn hàng này.");
            }

            return Ok(order);
        }

        [HttpPost("checkout-cart")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutFromCartRequestDTO dto)
        {
            try
            {
                var order = await _service.CheckoutFromCartAsync(GetCurrentCustomerId(), dto);
                if (order == null) return BadRequest("Thanh toán thất bại. Giỏ hàng trống hoặc dữ liệu không hợp lệ.");
                return Ok(order);
            }
            catch (Exception ex)
            {
                // Bắt lỗi Exception ném ra từ Database Transaction (Ví dụ: Trùng serial, hết hàng vật lý)
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout-direct")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutDirectly([FromBody] DirectCheckoutRequestDTO dto)
        {
            try
            {
                var order = await _service.CheckoutDirectlyAsync(GetCurrentCustomerId(), dto);
                if (order == null) return BadRequest("Thanh toán thất bại. Dữ liệu không hợp lệ.");
                return Ok(order);
            }
            catch (Exception ex)
            {
                // Bắt lỗi Exception ném ra từ Database Transaction
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDTO dto)
        {
            var result = await _service.UpdateOrderStatusAsync(id, dto);
            if (!result) return NotFound("Không tìm thấy đơn hàng để cập nhật.");

            return NoContent();
        }
    }
}