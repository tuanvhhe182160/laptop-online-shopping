using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebClient.ViewModels.User;

namespace WebClient.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<UserViewModel> Users { get; set; } = new();
        public List<RoleViewModel> Roles { get; set; } = new();

        public List<BranchViewModel> Branches { get; set; } = new();

        [BindProperty]
        public UserCreateViewModel NewUser { get; set; } = new();
        [BindProperty]
        public UserEditViewModel EditUser { get; set; } = new();
        [BindProperty]
        public UserFormViewModel UserForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = CreateAuthClient();

            // 1. Users
            var userResponse = await client.GetAsync("api/Users");
            if (userResponse.IsSuccessStatusCode)
            {
                var userContent = await userResponse.Content.ReadAsStringAsync();
                Users = JsonSerializer.Deserialize<List<UserViewModel>>(userContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            // 2. Roles
            var roleResponse = await client.GetAsync("api/Users/roles");
            if (roleResponse.IsSuccessStatusCode)
            {
                var roleContent = await roleResponse.Content.ReadAsStringAsync();
                Roles = JsonSerializer.Deserialize<List<RoleViewModel>>(roleContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            // 3. Branches
            var branchResponse = await client.GetAsync("api/Branches");
            if (branchResponse.IsSuccessStatusCode)
            {
                var branchContent = await branchResponse.Content.ReadAsStringAsync();
                Branches = JsonSerializer.Deserialize<List<BranchViewModel>>(branchContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var client = CreateAuthClient();
            var content = new StringContent(JsonSerializer.Serialize(UserForm), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Users", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Đã cấp tài khoản [{UserForm.Username}] thành công!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi tạo tài khoản: {error}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var client = CreateAuthClient();

            var content = new StringContent(
                JsonSerializer.Serialize(UserForm),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                $"api/Users/{UserForm.UserId}",
                content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật nhân viên [{UserForm.Username}]!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi cập nhật: {error}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var client = CreateAuthClient();
            var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/Users/{id}/toggle-status", emptyContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái của tài khoản #{id}!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Lỗi cập nhật: {error}";
            }
            return RedirectToPage();
        }

        private HttpClient CreateAuthClient()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }
    }

    public record BranchViewModel(int BranchId, string BranchName);
}