using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebClient.Models;

namespace WebClient.Pages.Storefront
{
    [Authorize(Roles = "Customer")]
    public class OrderHistoryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderHistoryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");

            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/Orders/my-orders");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = await JsonSerializer.DeserializeAsync<List<OrderViewModel>>(await response.Content.ReadAsStreamAsync(), options);
                if (list != null) Orders = list;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Nếu Token hết hạn hoặc bị lỗi, đá văng ra trang Login
                return RedirectToPage("/Auth/CustomerLogin");
            }

            return Page();
        }
    }
}
