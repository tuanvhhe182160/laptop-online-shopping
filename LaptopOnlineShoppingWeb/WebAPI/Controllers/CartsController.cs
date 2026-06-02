using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/customers/{customerId}/cart")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _service;

        public CartsController(ICartService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart(int customerId)
        {
            var cart = await _service.GetCartAsync(customerId);
            if (cart == null) return NotFound("Giỏ hàng không tồn tại.");
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(int customerId, [FromBody] AddToCartRequestDTO dto)
        {
            var result = await _service.AddToCartAsync(customerId, dto);
            if (!result) return BadRequest("Thêm vào giỏ hàng thất bại. Vượt quá tồn kho hoặc sai thông tin.");
            return Ok();
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateCartItem(int customerId, [FromBody] UpdateCartItemDTO dto)
        {
            var result = await _service.UpdateCartItemAsync(customerId, dto);
            if (!result) return BadRequest("Cập nhật thất bại.");
            return Ok();
        }

        [HttpDelete("items/{laptopId}")]
        public async Task<IActionResult> RemoveFromCart(int customerId, int laptopId)
        {
            var result = await _service.RemoveFromCartAsync(customerId, laptopId);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
