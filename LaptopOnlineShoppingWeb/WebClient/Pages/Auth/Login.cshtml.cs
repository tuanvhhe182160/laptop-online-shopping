using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebClient.ViewModels.Auth;

namespace WebClient.Pages.Auth
{
    public class LoginModel : PageModel
    {
        public string GoogleClientId { get; set; }
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            GoogleClientId = _configuration["Google:ClientId"];

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    Response.Redirect("/admin/dashboard");
                }
                else if (User.IsInRole("Staff"))
                {
                    Response.Redirect("/admin/orders"); // Địa bàn của Staff: Quản lý đơn hàng
                }
                else if (User.IsInRole("WarehouseManager"))
                {
                    Response.Redirect("/BackOffice/Warehouse/Import"); // Địa bàn của Manager: Quản lý kho nhập seri
                }
                else
                {
                    Response.Redirect("/Storefront/Index");
                }
            }
        }

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            // Xóa luôn token trong localStorage của JS khi Logout
            Response.Cookies.Delete("AccessToken");
            return RedirectToPage("/Auth/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var loginData = new { Username = this.Username, Password = this.Password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("WebAPI");

            var response = await client.PostAsync("api/Auth/staff-login", content);
            var responseData = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<LoginResponse>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, Username),
                        new Claim(ClaimTypes.Role, result.Role),
                        new Claim("FullName", result.FullName),
                        new Claim("AvatarUrl", result.AvatarUrl ?? ""),
                        new Claim("AccessToken", result.Token) 
                    };

                    if (result.BranchId.HasValue)
                    {
                        claims.Add(new Claim("BranchId", result.BranchId.Value.ToString()));
                    }
                    else
                    {
                        claims.Add(new Claim("BranchId", "0")); // Admin tổng
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
                    var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3) };

                    await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

                    string defaultUrl = "/Storefront/Index"; // Mặc định cho Customer

                    if (result.Role == "Admin")
                    {
                        defaultUrl = "/admin/dashboard";
                    }
                    else if (result.Role == "Staff")
                    {
                        defaultUrl = "/admin/orders"; // Đường dẫn đến trang quản lý đơn hàng của Mem 3
                    }
                    else if (result.Role == "WarehouseManager")
                    {
                        defaultUrl = "/BackOffice/Warehouse/Import"; // Đường dẫn đến trang nhập kho của Mem 2
                    }

                    string targetUrl = ReturnUrl ?? defaultUrl;
                    return LocalRedirect(targetUrl);
                }
            }
            try
            {
                var errorObj = JsonSerializer.Deserialize<ErrorResponse>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ErrorMessage = errorObj?.Message ?? "Đăng nhập thất bại.";
            }
            catch
            {
                ErrorMessage = "Lỗi kết nối đến máy chủ API.";
            }

            return Page();
        }
    }
}