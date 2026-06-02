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
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
            if(User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Staff")))
            {
                Response.Redirect("/Admin/Index");
            }
        }

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
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

                    var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                    };

                    await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);
                    return LocalRedirect(ReturnUrl ?? "/Admin/Index");
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
