using Azure;
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

        [BindProperty]
        public UserCreateViewModel NewUser { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = CreateAuthClient();

            var userResponse = await client.GetAsync("api/Users");
            if (userResponse.IsSuccessStatusCode)
            {
                var userContent = await userResponse.Content.ReadAsStringAsync();
                Users = JsonSerializer.Deserialize<List<UserViewModel>>(userContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            var roleResponse = await client.GetAsync("api/Users/roles");
            if (roleResponse.IsSuccessStatusCode)
            {
                var roleContent = await roleResponse.Content.ReadAsStringAsync();
                Roles = JsonSerializer.Deserialize<List<RoleViewModel>>(roleContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var client = CreateAuthClient();
            var content = new StringContent(JsonSerializer.Serialize(NewUser), Encoding.UTF8, "application/json");

            await client.PostAsync("api/Users", content);
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
                TempData["ErrorMessage"] = $"Lỗi cập nhật (Mã {(int)response.StatusCode}): {error}";
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
}
