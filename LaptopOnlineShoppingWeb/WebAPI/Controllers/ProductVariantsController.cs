using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductVariantRepository _variantRepository;

        public ProductVariantsController(IProductService productService, IProductVariantRepository variantRepository)
        {
            _productService = productService;
            _variantRepository = variantRepository;
        }

        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult GetAll()
        {
            return Ok(_variantRepository.GetQueryable());
        }

        [HttpGet("{id}")]
        [EnableQuery(MaxExpansionDepth = 3)]
        public async Task<IActionResult> GetById(int id)
        {
            var variant = await _variantRepository.GetByIdAsync(id);
            if (variant == null) return NotFound(new { message = "Không tìm thấy biến thể." });
            return Ok(variant);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateProductVariantDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var variant = await _productService.CreateVariantAsync(request);
            return Ok(new { message = "Thêm biến thể thành công!", data = variant });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductVariantDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _productService.UpdateVariantAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy biến thể." });
            return Ok(new { message = "Cập nhật biến thể thành công!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteVariantAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy biến thể." });
            return Ok(new { message = "Xóa biến thể thành công!" });
        }
    }
}
