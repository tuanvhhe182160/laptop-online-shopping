using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/Cart")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _service;

        public CartsController(ICartService service)
        {
            _service = service;
        }

        private int GetCurrentCustomerId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdStr!);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int customerId = GetCurrentCustomerId();
            var cart = await _service.GetCartAsync(customerId);
            if (cart == null) return NotFound("Giỏ hàng không tồn tại.");
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDTO dto)
        {
            int customerId = GetCurrentCustomerId();
            var result = await _service.AddToCartAsync(customerId, dto);
            if (!result) return BadRequest("Thêm vào giỏ hàng thất bại. Vượt quá tồn kho hoặc sai thông tin.");
            return Ok();
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDTO dto)
        {
            int customerId = GetCurrentCustomerId();
            var result = await _service.UpdateCartItemAsync(customerId, dto);
            if (!result) return BadRequest("Cập nhật thất bại.");
            return Ok();
        }

        [HttpDelete("items/{variantId}")]
        public async Task<IActionResult> RemoveFromCart(int variantId)
        {
            int customerId = GetCurrentCustomerId();
            var result = await _service.RemoveFromCartAsync(customerId, variantId);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
