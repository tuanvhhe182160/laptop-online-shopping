using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace WebClient.Pages.Auth
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public bool IsGoogleUser { get; set; }

        public void OnGet()
        {
            var isGoogleClaim = User.FindFirst("IsGoogleAccount")?.Value;

            IsGoogleUser = (isGoogleClaim == "true");
        }

        public async Task<IActionResult> OnPutChangePasswordAsync([FromBody] ChangePasswordBindingModel model)
        {
            // 1. Lấy AccessToken được giấu trong Cookie ra
            var accessToken = User.FindFirstValue("AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode(401, new { message = "Phiên đăng nhập đã hết hạn." });
            }

            // 2. Tạo HttpClient để "đi đêm" sang Web API
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7136/"); // URL của Web API
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // 3. Chuẩn bị dữ liệu gửi đi
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            // 4. Gọi sang Web API thật
            var response = await client.PutAsync("api/Profile/change-password", jsonContent);
            var responseData = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // NẾU ĐỔI THÀNH CÔNG: Nếu trước đó là tài khoản Google, nay đã có mật khẩu,
                // Bạn có thể cần cập nhật lại Cookie để biến IsGoogle thành false.
                return new JsonResult(new { success = true, message = "Cập nhật mật khẩu thành công!" });
            }

            // Nếu thất bại, trả lỗi của API về cho Javascript hiển thị
            var errorMsg = "Đổi mật khẩu thất bại.";
            try
            {
                var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(responseData);
                if (errorObj != null && errorObj.ContainsKey("message")) errorMsg = errorObj["message"];
            }
            catch { }

            return new JsonResult(new { success = false, message = errorMsg });
        }
    }

    public class ChangePasswordBindingModel
    {
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
