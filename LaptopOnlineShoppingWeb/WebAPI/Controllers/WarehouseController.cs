using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "WarehouseManager")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] ImportPhysicalProductDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Trích xuất BranchId từ Claim JWT
            var branchIdClaim = User.FindFirst("BranchId")?.Value;
            if (string.IsNullOrEmpty(branchIdClaim) || !int.TryParse(branchIdClaim, out int branchId) || branchId <= 0)
            {
                return BadRequest(new { message = "Bạn không có quyền nhập kho do không thuộc chi nhánh hợp lệ." });
            }

            var result = await _warehouseService.ImportProductsAsync(request, branchId);
            
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}
