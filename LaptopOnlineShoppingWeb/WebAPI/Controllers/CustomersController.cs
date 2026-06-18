using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using WebAPI.DTOs;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly ICustomerRepository _customerRepository;

        public CustomersController(ICustomerService service, ICustomerRepository customerRepository)
        {
            _service = service;
            _customerRepository = customerRepository;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> Get()
        {
            var customers = await _service.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _service.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerCreateDTO dto)
        {
            var customer = await _service.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = customer.CustomerId }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CustomerUpdateDTO dto)
        {
            var result = await _service.UpdateCustomerAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteCustomerAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/upload-avatar")]
        // Có thể mở rộng cho phép chính User đó tự đổi avatar của mình
        public async Task<IActionResult> UploadAvatar(int id, IFormFile file)
        {
            // 1. Kiểm tra file có tồn tại không
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một tệp hình ảnh." });

            // 2. Validate Dung lượng (Tối đa 2MB = 2 * 1024 * 1024 bytes)
            if (file.Length > 2097152)
                return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });

            // 3. Validate Đuôi file (Extension)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Chỉ chấp nhận định dạng .jpg, .jpeg hoặc .png." });

            // 4. Validate MIME Type (Chống Fake đuôi file)
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Định dạng tệp không hợp lệ." });

            // 5. Tìm Customer trong DB
            var customer = await _customerRepository.GetByIdAsync(id); // Nhớ đổi hàm này theo Repo của bạn
            if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng." });

            // 6. Xử lý lưu File vật lý vào thư mục wwwroot/uploads/avatars
            // LƯU Ý: Đảm bảo project WebAPI của bạn có thư mục wwwroot/uploads/avatars
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file ngẫu nhiên để không bị trùng (vd: customer_1_123456789.jpg)
            string uniqueFileName = $"customer_{id}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 7. Lưu đường dẫn tương đối vào DB
            string avatarUrl = $"/uploads/avatars/{uniqueFileName}";
            customer.AvatarUrl = avatarUrl;

            _customerRepository.Update(customer);
            await _customerRepository.SaveAsync();

            return Ok(new { message = "Cập nhật ảnh đại diện thành công!", avatarUrl = avatarUrl });
        }
    }
}
