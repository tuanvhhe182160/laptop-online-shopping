using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Repositories;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,WarehouseManager")]
    public class PhysicalProductsController : ControllerBase
    {
        private readonly IPhysicalProductRepository _repo;

        public PhysicalProductsController(IPhysicalProductRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 4)]
        public IActionResult GetAll()
        {
            return Ok(_repo.GetQueryable());
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus)) return BadRequest("Trạng thái không hợp lệ.");

            var product = await _repo.GetByIdAsync(id);
            if (product == null) return NotFound(new { message = "Không tìm thấy seri này." });

            product.Status = newStatus;
            _repo.Update(product);
            await _repo.SaveAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }
    }
}
