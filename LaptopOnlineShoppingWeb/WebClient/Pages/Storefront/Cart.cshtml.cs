using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebClient.Models;
using System.Security.Claims;

namespace WebClient.Pages.Storefront
{
    public class CartModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CartModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public CartViewModel? Cart { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy ID Khách hàng từ Cookie
            var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(customerIdStr, out int customerId))
            {
                // Nếu chưa đăng nhập, giả sử 1 cho mục đích test
                customerId = 1; 
            }

            var client = _httpClientFactory.CreateClient("WebAPI");
            var response = await client.GetAsync($"/api/customers/{customerId}/cart");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cart = await JsonSerializer.DeserializeAsync<CartViewModel>(await response.Content.ReadAsStreamAsync(), options);
            }

            return Page();
        }
    }
}
