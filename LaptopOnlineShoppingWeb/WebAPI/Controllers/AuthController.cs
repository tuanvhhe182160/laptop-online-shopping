using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Data;
using WebAPI.DTOs.Auth;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("staff-login")]
        public async Task<IActionResult> StaffLogin([FromBody] LoginRequest request)
        {
            var adminUser = _configuration["AdminAccount:Username"];
            var adminPass = _configuration["AdminAccount:Password"];

            if (request.Username == adminUser && request.Password == adminPass)
            {
                var adminToken = GenerateJwtToken("0", adminUser!, "Admin", _configuration["AdminAccount:FullName"]!, null);
                return Ok(new LoginResponse
                {
                    Token = adminToken,
                    FullName = _configuration["AdminAccount:FullName"]!,
                    Role = "Admin",
                    AvatarUrl = ""
                });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive == true);

            if (user == null) return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid) return Unauthorized(new { message = "Mật khẩu không chính xác." });

            var token = GenerateJwtToken(user.UserId.ToString(), user.Username, user.Role.RoleName, user.FullName, user.AvatarUrl);

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

            var token = GenerateJwtToken(customer.CustomerId.ToString(), customer.Username, "Customer", customer.FullName, null);

            return Ok(new LoginResponse
            {
                Token = token,
                FullName = customer.FullName,
                Role = "Customer",
                AvatarUrl = ""
            });
        }

        private string GenerateJwtToken(string id, string username, string role, string fullName, string? avatarUrl)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", fullName),
                new Claim("AvatarUrl", avatarUrl ?? "")
            };

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
    }
}
