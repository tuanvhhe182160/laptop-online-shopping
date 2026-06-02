using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebClient.Models;
using System.Security.Claims;

namespace WebClient.Pages.Storefront
{
    public class CheckoutModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckoutModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public CartViewModel? CartSummary { get; set; }
        public OrderDetailViewModel? DirectPurchaseItem { get; set; }

        public bool IsBuyNow { get; set; }
        public int LaptopId { get; set; }
        public int Quantity { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] bool buynow = false, [FromQuery] int laptopid = 0, [FromQuery] int qty = 0)
        {
            var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int customerId = int.TryParse(customerIdStr, out int id) ? id : 1; // Test mock

            IsBuyNow = buynow;
            LaptopId = laptopid;
            Quantity = qty;

            var client = _httpClientFactory.CreateClient("WebAPI");

            if (IsBuyNow && LaptopId > 0 && Quantity > 0)
            {
                // Gọi API lấy thông tin laptop nếu cần, hoặc tạo mockup
                // Ở đây mô phỏng lấy tên giá từ API (giả sử có api/laptops)
                var response = await client.GetAsync($"/api/laptops/{LaptopId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    var laptopName = doc.RootElement.GetProperty("laptopName").GetString();
                    var price = doc.RootElement.GetProperty("price").GetDecimal();

                    DirectPurchaseItem = new OrderDetailViewModel
                    {
                        LaptopId = LaptopId,
                        LaptopName = laptopName ?? "Laptop",
                        UnitPrice = price,
                        Quantity = Quantity,
                        TotalPrice = price * Quantity
                    };
                }
            }
            else
            {
                // Checkout từ Giỏ hàng
                var response = await client.GetAsync($"/api/customers/{customerId}/cart");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    CartSummary = await JsonSerializer.DeserializeAsync<CartViewModel>(await response.Content.ReadAsStreamAsync(), options);
                }
            }

            return Page();
        }
    }
}
