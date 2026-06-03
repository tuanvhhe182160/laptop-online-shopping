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
                RoleName = u.Role?.RoleName 
            });

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                return BadRequest("Username đã tồn tại."); // Bỏ object nặc danh để Client đọc chuỗi lỗi dễ hơn

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                RoleId = request.RoleId,
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
                RoleName = user.Role?.RoleName ?? "Chưa cấp quyền"
            };

            return Ok(response);
        }
    }
}