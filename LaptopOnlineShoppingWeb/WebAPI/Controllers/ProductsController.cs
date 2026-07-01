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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductService productService, IProductRepository productRepository)
        {
            _productService = productService;
            _productRepository = productRepository;
        }

        [HttpGet]
        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult GetAll()
        {
            return Ok(_productRepository.GetQueryable());
        }

        [HttpGet("{id}")]
        [EnableQuery(MaxExpansionDepth = 3)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var product = await _productService.CreateProductAsync(request);
            return Ok(new { message = "Thêm sản phẩm thành công!", data = product });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _productService.UpdateProductAsync(id, request);
            if (!success) return NotFound(new { message = "Không tìm thấy sản phẩm." });
            return Ok(new { message = "Cập nhật sản phẩm thành công!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy sản phẩm." });
            return Ok(new { message = "Xóa sản phẩm thành công!" });
        }
    }
}
