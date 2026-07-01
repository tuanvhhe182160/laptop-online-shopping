using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAPI.Data;
using WebAPI.DTOs.User;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;

            if (userId == 0 && role == "Admin")
            {
                return Ok(new
                {
                    FullName = "Quản trị viên tối cao",
                    Email = "admin@laptopshop.com",
                    AvatarUrl = "",
                    RoleName = "Admin",
                    BranchName = "Toàn quyền Server"
                });
            }

            if (role == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);
                if (customer == null) return NotFound("Không tìm thấy dữ liệu khách hàng.");
                return Ok(new
                {
                    customer.FullName,
                    customer.Email,
                    customer.Phone,
                    customer.Address,
                    customer.AvatarUrl
                });
            }
            else // Admin, Staff, WarehouseManager
            {
                var user = await _context.Users.Include(u => u.Branch).FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null) return NotFound("Không tìm thấy dữ liệu nhân viên.");
                return Ok(new
                {
                    user.FullName,
                    user.Email,
                    user.AvatarUrl,
                    RoleName = role,
                    BranchName = user.Branch?.BranchName ?? "Toàn hệ thống"
                });
            }
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;

            if (userId == 0 && role == "Admin")
                return BadRequest(new { message = "Tài khoản Admin gốc cấu hình trong tệp hệ thống không thể sửa đổi." });

            if (role == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);
                if (customer == null) return NotFound(new { message = "Tài khoản không tồn tại." });

                customer.FullName = request.FullName;
                customer.Phone = request.Phone;
                customer.Address = request.Address;
            }
            else
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound(new { message = "Tài khoản không tồn tại." });

                user.FullName = request.FullName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ thành công!" });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;

            if (userId == 0 && role == "Admin")
                return BadRequest(new { message = "Không thể đổi mật khẩu của tài khoản Admin hệ thống qua API." });

            if (role == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);

                if (customer == null) return NotFound(new { message = "Tài khoản không tồn tại." });
                if (customer.IsGoogleAccount)
                {
                    customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                    customer.IsGoogleAccount = false;
                }
                else
                {
                    if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, customer.PasswordHash))
                        return BadRequest(new { message = "Mật khẩu cũ không chính xác." });

                    customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                }           
            }
            else
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound(new { message = "Nhân viên không tồn tại." });

                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                    return BadRequest(new { message = "Mật khẩu cũ không chính xác." });

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            string role = User.FindFirstValue(ClaimTypes.Role)!;

            if (userId == 0 && role == "Admin")
                return BadRequest(new { message = "Tài khoản Admin hệ thống không hỗ trợ đổi Avatar." });

            // 1. Kiểm tra file
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một tệp hình ảnh." });

            // 2. Giới hạn 2MB
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });

            // 3. Validate Extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Chỉ chấp nhận file .jpg, .jpeg hoặc .png." });

            // 4. Validate MIME Type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };

            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(new { message = "Định dạng tệp không hợp lệ." });

            // 5. Tạo thư mục uploads nếu chưa tồn tại
            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "avatars");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 6. Tạo file mới
            string uniqueFileName = $"user_{role}_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string newAvatarUrl = $"/uploads/avatars/{uniqueFileName}";

            if (role == "Customer")
            {
                var customer = await _context.Customers.FindAsync(userId);

                if (customer == null)
                    return NotFound(new { message = "Không tìm thấy khách hàng." });

                // Xóa avatar cũ nếu là file local
                if (!string.IsNullOrEmpty(customer.AvatarUrl)
                    && customer.AvatarUrl.StartsWith("/uploads/"))
                {
                    string oldFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        customer.AvatarUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                customer.AvatarUrl = newAvatarUrl;
            }
            else // ADMIN / STAFF / WAREHOUSE MANAGER
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return NotFound(new { message = "Không tìm thấy người dùng." });

                // Xóa avatar cũ nếu là file local
                if (!string.IsNullOrEmpty(user.AvatarUrl)
                    && user.AvatarUrl.StartsWith("/uploads/"))
                {
                    string oldFilePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        user.AvatarUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.AvatarUrl = newAvatarUrl;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật ảnh đại diện thành công!",
                avatarUrl = newAvatarUrl
            });
        }
    }
}
