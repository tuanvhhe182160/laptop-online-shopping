using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebAPI.Enums;
using WebClient.Models;

namespace WebClient.Pages.Admin.Orders
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<OrderViewModel> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");

            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/Orders?$orderby=OrderDate desc");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var list = await JsonSerializer.DeserializeAsync<List<OrderViewModel>>(await response.Content.ReadAsStreamAsync(), options);
                if (list != null) Orders = list;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                     response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return RedirectToPage("/Auth/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus status)
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Payload truyền đi sẽ tự convert enum thành string hoặc int tương ứng với DTO ở API
            var payload = new { OrderStatus = status.ToString() }; 
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/Orders/{orderId}/status")
            {
                Content = content
            };

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} thành {status}!";
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                var apiError = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Mã lỗi {statusCode}: {(!string.IsNullOrEmpty(apiError) ? apiError : "Không rõ nguyên nhân từ API.")}";
            }
            return RedirectToPage();
        }
    }
}