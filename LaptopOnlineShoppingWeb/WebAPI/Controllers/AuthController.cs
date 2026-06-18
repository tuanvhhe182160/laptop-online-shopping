using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Data;
using WebAPI.DTOs.Auth;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin([FromBody] LoginRequest request)
        {
            var adminUser = _configuration["AdminAccount:Username"];
            var adminPass = _configuration["AdminAccount:Password"];

            // 1. Check Hardcoded Admin
            if (request.Username == adminUser && request.Password == adminPass)
            {
                var adminToken = GenerateJwtToken("0", adminUser!, "Admin", _configuration["AdminAccount:FullName"]!, null, null);
                return Ok(new LoginResponse
                {
                    Token = adminToken,
                    FullName = _configuration["AdminAccount:FullName"]!,
                    Role = "Admin",
                    AvatarUrl = ""
                });
            }

            // 2. Check DB Users
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive == true);

            if (user == null) return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid) return Unauthorized(new { message = "Mật khẩu không chính xác." });

            // Truyền BranchId vào Token
            var token = GenerateJwtToken(user.UserId.ToString(), user.Username, user.Role.RoleName, user.FullName, user.AvatarUrl, user.BranchId);

            return Ok(new LoginResponse
            {
                Token = token,
                FullName = user.FullName,
                Role = user.Role.RoleName,
                AvatarUrl = user.AvatarUrl ?? ""
            });
        }

        [HttpPost("customer-register")]
        public async Task<IActionResult> CustomerRegister([FromBody] RegisterRequest request)
        {
            if (await _context.Customers.AnyAsync(c => c.Username == request.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại trong hệ thống." });
            }
            if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest(new { message = "Email này đã được sử dụng." });
            }

            string saltPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newCustomer = new Customer
            {
                Username = request.Username,
                PasswordHash = saltPasswordHash,
                FullName = request.FullName,
                Email = request.Email,
                IsActive = true
            };

            await _context.Customers.AddAsync(newCustomer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        [HttpPost("customer-login")]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginRequest request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == request.Username && c.IsActive == true);

            if (customer == null)
            {
                return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Sai mật khẩu." });
            }

            // Customer không thuộc chi nhánh nào nên truyền BranchId = null
            var token = GenerateJwtToken(customer.CustomerId.ToString(), customer.Username, "Customer", customer.FullName, customer.AvatarUrl, null);

            return Ok(new LoginResponse
            {
                Token = token,
                FullName = customer.FullName,
                Role = "Customer",
                AvatarUrl = customer.AvatarUrl ?? ""
            });
        }

        private string GenerateJwtToken(string id, string username, string role, string fullName, string? avatarUrl, int? branchId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", fullName),
                new Claim("AvatarUrl", avatarUrl ?? "")
            };

            if (branchId.HasValue)
            {
                claims.Add(new Claim("BranchId", branchId.Value.ToString()));
            }
            else
            {
                // Quy ước: BranchId = 0 là không bị giới hạn chi nhánh (Dành cho Admin tổng hoặc Customer)
                claims.Add(new Claim("BranchId", "0"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { message = "Vui lòng nhập email." });

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return BadRequest(new { message = "Email không tồn tại trong hệ thống." });

            // 1. Sinh Token ngẫu nhiên (6 số)
            string resetToken = new Random().Next(100000, 999999).ToString();

            // 2. Lưu vào DB kèm thời hạn 15 phút
            customer.ResetPasswordToken = resetToken;
            customer.ResetPasswordExpiry = DateTime.Now.AddMinutes(15);
            await _context.SaveChangesAsync();

            // 3. Gửi Email chứa Token
            string subject = "Yêu cầu khôi phục mật khẩu";
            string body = $@"
                <h3>Xin chào {customer.FullName},</h3>
                <p>Bạn đã yêu cầu đặt lại mật khẩu tại LaptopShop.</p>
                <p>Mã xác nhận của bạn là: <strong style='font-size:24px; color:blue;'>{resetToken}</strong></p>
                <p>Mã này sẽ hết hạn sau 15 phút. Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

            try
            {
                await _emailService.SendEmailAsync(customer.Email, subject, body);
                return Ok(new { message = "Mã xác nhận đã được gửi đến email của bạn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gửi email: " + ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

            if (customer == null)
                return BadRequest(new { message = "Email không hợp lệ." });

            if (customer.ResetPasswordToken != request.Token)
                return BadRequest(new { message = "Mã xác nhận không chính xác." });

            if (customer.ResetPasswordExpiry < DateTime.Now)
                return BadRequest(new { message = "Mã xác nhận đã hết hạn. Vui lòng yêu cầu mã mới." });

            // Cập nhật mật khẩu mới
            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // Xóa token sau khi dùng xong
            customer.ResetPasswordToken = null;
            customer.ResetPasswordExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới." });
        }
    }
}
