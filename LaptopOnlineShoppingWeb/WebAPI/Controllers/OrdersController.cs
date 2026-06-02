using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var orders = await _service.GetOrdersByCustomerAsync(customerId);
            return Ok(orders);
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _service.GetOrderDetailsAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost("checkout-cart/{customerId}")]
        public async Task<IActionResult> CheckoutFromCart(int customerId, [FromBody] CheckoutFromCartRequestDTO dto)
        {
            var order = await _service.CheckoutFromCartAsync(customerId, dto);
            if (order == null) return BadRequest("Thanh toán thất bại. Giỏ hàng trống hoặc có sản phẩm vượt quá tồn kho.");
            return Ok(order);
        }

        [HttpPost("checkout-direct/{customerId}")]
        public async Task<IActionResult> CheckoutDirectly(int customerId, [FromBody] DirectCheckoutRequestDTO dto)
        {
            var order = await _service.CheckoutDirectlyAsync(customerId, dto);
            if (order == null) return BadRequest("Thanh toán thất bại. Sản phẩm vượt quá tồn kho.");
            return Ok(order);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDTO dto)
        {
            var result = await _service.UpdateOrderStatusAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
