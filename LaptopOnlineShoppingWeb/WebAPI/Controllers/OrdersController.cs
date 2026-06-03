using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Security.Claims;
using System.Threading.Tasks;
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
        [EnableQuery]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("my-orders")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders()
        {
            var orders = await _service.GetOrdersByCustomerAsync(GetCurrentCustomerId());
            return Ok(orders);
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _service.GetOrderDetailsAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost("checkout-cart")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutFromCartRequestDTO dto)
        {
            var order = await _service.CheckoutFromCartAsync(GetCurrentCustomerId(), dto);
            if (order == null) return BadRequest("Thanh toán thất bại. Giỏ hàng trống hoặc có sản phẩm vượt quá tồn kho.");
            return Ok(order);
        }

        [HttpPost("checkout-direct")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CheckoutDirectly([FromBody] DirectCheckoutRequestDTO dto)
        {
            var order = await _service.CheckoutDirectlyAsync(GetCurrentCustomerId(), dto);
            if (order == null) return BadRequest("Thanh toán thất bại. Sản phẩm vượt quá tồn kho.");
            return Ok(order);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDTO dto)
        {
            var result = await _service.UpdateOrderStatusAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
