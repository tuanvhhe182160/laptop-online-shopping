using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using WebAPI.DTOs.Categories;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class CategoriesController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IProductRepository _productRepository; 

        public CategoriesController(IGenericRepository<Category> categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetAll()
        {
            return Ok(_categoryRepository.GetQueryable());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound(new { message = "Không tìm thấy hãng này." });
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDTO request)
        {
            var existing = await _categoryRepository.FindAsync(c => c.CategoryName == request.CategoryName);
            if (existing != null)
                return BadRequest(new { message = "Tên hãng này đã tồn tại." });

            var category = new Category
            {
                CategoryName = request.CategoryName,
                Description = request.Description
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return Ok(new { message = "Thêm danh mục hãng thành công!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDTO request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound(new { message = "Không tìm thấy hãng này." });

            category.CategoryName = request.CategoryName;
            category.Description = request.Description;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();

            return Ok(new { message = "Cập nhật thông tin thành công!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound(new { message = "Không tìm thấy hãng." });

            bool hasProducts = _productRepository.GetQueryable().Any(l => l.CategoryId == id);
            if (hasProducts) return BadRequest(new { message = "Không thể xóa hãng này vì có sản phẩm liên kết." });

            _categoryRepository.Delete(category); 
            await _categoryRepository.SaveAsync();

            return Ok(new { message = "Xóa danh mục hãng thành công!" });
        }
    }
}
