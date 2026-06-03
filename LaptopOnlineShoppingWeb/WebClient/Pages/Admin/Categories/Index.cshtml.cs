using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebClient.ViewModels.Auth;
using WebClient.ViewModels.Category;

namespace WebClient.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CategoryViewModel> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty]
        public CategoryCrudViewModel CategoryForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = CreateAuthClient();
            string url = "api/Categories";

            if (!string.IsNullOrEmpty(SearchName))
            {
                url += $"?$filter=contains(CategoryName, '{SearchName}')";
            }

            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        // Xử lý Thêm mới hoặc Chỉnh sửa qua Pop-up
        public async Task<IActionResult> OnPostSaveAsync()
        {
            var client = CreateAuthClient();
            var jsonContent = new StringContent(JsonSerializer.Serialize(CategoryForm), Encoding.UTF8, "application/json");
            HttpResponseMessage response;

            if (CategoryForm.CategoryId == 0)
            {
                response = await client.PostAsync("api/Categories", jsonContent);
            }
            else
            {
                response = await client.PutAsync($"api/Categories/{CategoryForm.CategoryId}", jsonContent);
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Lưu thông tin Hãng thành công!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                HandleError(errorContent);
            }

            return RedirectToPage();
        }

        // Xử lý Xóa (Chỉ Admin lọt qua bộ lọc Authorize ở API)
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = CreateAuthClient();
            var response = await client.DeleteAsync($"api/Categories/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xóa Hãng thành công!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                HandleError(errorContent);
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

        private void HandleError(string errorContent)
        {
            try
            {
                var errorObj = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TempData["ErrorMessage"] = errorObj?.Message ?? "Thao tác thất bại.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Lỗi kết nối hoặc xung đột hệ thống.";
            }
        }
    }
}
