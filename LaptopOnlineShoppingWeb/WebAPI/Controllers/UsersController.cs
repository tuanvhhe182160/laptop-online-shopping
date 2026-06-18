using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.User;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UsersController(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync();

            // Map dữ liệu ở tầng Controller
            var response = users.Select(u => new
            {
                u.UserId,
                u.Username,
                u.FullName,
                u.Email,
                u.IsActive,
                RoleName = u.Role?.RoleName,
                BranchId = u.BranchId,
                BranchName = u.Branch?.BranchName ?? "Toàn hệ thống" // Hiển thị cho Admin tổng
            });

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                return BadRequest("Username đã tồn tại."); 

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                RoleId = request.RoleId,
                BranchId = request.BranchId,
                IsActive = request.IsActive
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.SaveAsync();

            return Ok(new { message = "Thêm mới thành công!" });
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _userRepository.ToggleStatusAsync(id);
            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return Ok(roles);
        }
        

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userRepository.GetUserDetailsAsync(id);
            if (user == null) return NotFound("Không tìm thấy nhân viên.");

            // Format lại dữ liệu trả về
            var response = new
            {
                user.UserId,
                user.Username,
                user.FullName,
                user.Email,
                user.IsActive,
                RoleName = user.Role?.RoleName ?? "Chưa cấp quyền",
                BranchId = user.BranchId,
                BranchName = user.Branch?.BranchName ?? "Toàn hệ thống"
            };

            return Ok(response);
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

            // 5. Tìm User trong DB
            var user = await _userRepository.GetByIdAsync(id); // Nhớ đổi hàm này theo Repo của bạn
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            // 6. Xử lý lưu File vật lý vào thư mục wwwroot/uploads/avatars
            // LƯU Ý: Đảm bảo project WebAPI của bạn có thư mục wwwroot/uploads/avatars
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file ngẫu nhiên để không bị trùng (vd: user_1_123456789.jpg)
            string uniqueFileName = $"user_{id}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 7. Lưu đường dẫn tương đối vào DB
            string avatarUrl = $"/uploads/avatars/{uniqueFileName}";
            user.AvatarUrl = avatarUrl;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveAsync();

            return Ok(new { message = "Cập nhật ảnh đại diện thành công!", avatarUrl = avatarUrl });
        }
    }
}