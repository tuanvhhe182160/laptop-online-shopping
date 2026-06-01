using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WebClient.ViewModels.Auth;

namespace WebClient.Pages.Auth
{
    public class CustomerRegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerRegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;
        [BindProperty]
        public string Password { get; set; } = string.Empty;
        [BindProperty]
        public string FullName { get; set; } = string.Empty;
        [BindProperty]
        public string Email { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var registerData = new { Username, Password, FullName, Email };
            var content = new StringContent(JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient("WebAPI");
            var response = await client.PostAsync("api/auth/customer-register", content);

            var responseData = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToPage("/auth/customerlogin", new { success = "true" });
            }

            try
            {
                var errorObj = JsonSerializer.Deserialize<ErrorResponse>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ErrorMessage = errorObj?.Message ?? "Đăng ký thất bại.";
            }
            catch
            {
                ErrorMessage = "Lỗi kết nối đến máy chủ.";
            }

            return Page();
        }
    }
}
