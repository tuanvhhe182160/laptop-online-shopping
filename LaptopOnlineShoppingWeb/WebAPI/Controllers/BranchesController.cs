using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,WarehouseManager,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var branches = await _context.Branches.AsNoTracking().ToListAsync();
            return Ok(branches);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,WarehouseManager,Staff")]
        public async Task<IActionResult> GetById(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound(new { message = "Không tìm thấy chi nhánh." });
            return Ok(branch);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Branch request)
        {
            if (string.IsNullOrWhiteSpace(request.BranchName))
                return BadRequest(new { message = "Tên chi nhánh không được để trống." });

            _context.Branches.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm chi nhánh thành công!", branchId = request.BranchId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Branch request)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound(new { message = "Không tìm thấy chi nhánh." });

            branch.BranchName = request.BranchName;
            branch.Address = request.Address;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật chi nhánh thành công!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound(new { message = "Không tìm thấy chi nhánh." });

            // Kiểm tra xem chi nhánh có đang chứa hàng (PhysicalProduct) hoặc Nhân viên không
            bool hasUsers = await _context.Users.AnyAsync(u => u.BranchId == id);
            bool hasProducts = await _context.PhysicalProducts.AnyAsync(p => p.BranchId == id);

            if (hasUsers || hasProducts)
                return BadRequest(new { message = "Không thể xóa! Chi nhánh này đang có nhân viên hoặc sản phẩm trong kho." });

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa chi nhánh thành công!" });
        }
    }
}
