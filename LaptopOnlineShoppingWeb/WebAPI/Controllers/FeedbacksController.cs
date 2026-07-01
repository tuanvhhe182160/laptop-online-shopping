using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IFeedbackRepository _feedbackRepo;

        public FeedbacksController(IFeedbackService feedbackService, IFeedbackRepository feedbackRepo)
        {
            _feedbackService = feedbackService;
            _feedbackRepo = feedbackRepo;
        }

        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult GetAll()
        {
            // Nếu là Staff/Admin thì trả về tất cả
            if (User.Identity != null && User.Identity.IsAuthenticated && 
                (User.IsInRole("Admin") || User.IsInRole("Staff")))
            {
                return Ok(_feedbackRepo.GetQueryable());
            }

            // Nếu là Khách hàng hoặc vãng lai, chỉ trả về các feedback chưa bị ẩn
            var visibleFeedbacks = _feedbackRepo.GetQueryable()
                .Where(f => f.IsHidden == false || f.IsHidden == null);

            return Ok(visibleFeedbacks);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Tự động lấy CustomerId từ NameIdentifier claim của user đang đăng nhập
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int customerId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính khách hàng." });
            }

            var result = await _feedbackService.CreateFeedbackAsync(request, customerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _feedbackService.HideFeedbackAsync(id);

            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpPut("customer/{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] UpdateFeedbackDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int customerId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính khách hàng." });
            }

            var result = await _feedbackService.UpdateFeedbackAsync(id, customerId, request.Rating, request.Comment);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpDelete("customer/{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int customerId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính khách hàng." });
            }

            var result = await _feedbackService.DeleteFeedbackAsync(id, customerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}
