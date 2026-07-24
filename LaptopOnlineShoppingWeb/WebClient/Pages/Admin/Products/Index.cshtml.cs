using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.IO;
using WebClient.ViewModels.Category;

namespace WebClient.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _env;

        public IndexModel(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _env = env;
        }

        public List<CategoryViewModel> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            var token = User.FindFirst("AccessToken")?.Value;
            var client = _httpClientFactory.CreateClient("WebAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            var response = await client.GetAsync("api/Categories");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }

        public async Task<IActionResult> OnPostUploadImageAsync(IFormFile imageFile, string productCode, int imageIndex = 0)
        {
            try 
            {
                if (imageFile == null || string.IsNullOrWhiteSpace(productCode))
                {
                    return Content("{\"success\":false,\"message\":\"Dữ liệu không hợp lệ.\"}", "application/json");
                }

                var extension = Path.GetExtension(imageFile.FileName)?.ToLower() ?? "";
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    return Content("{\"success\":false,\"message\":\"Chỉ hỗ trợ file .jpg, .png\"}", "application/json");
                }

                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var folderPath = Path.Combine(webRoot, "images", "products");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Make productCode safe
                var safeProductCode = string.Join("_", productCode.Split(Path.GetInvalidFileNameChars()));
                var suffix = imageIndex == 0 ? "" : $"-{imageIndex}";
                var fileName = $"{safeProductCode}{suffix}.jpg";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return Content("{\"success\":true,\"message\":\"Cập nhật ảnh thành công!\"}", "application/json");
            } 
            catch (Exception ex) 
            {
                return Content("{\"success\":false,\"message\":\"Lỗi hệ thống: " + ex.Message.Replace("\"", "'") + "\"}", "application/json");
            }
        }
    }
}
