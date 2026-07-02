using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using WebClient.Models;

namespace WebClient.Pages.Storefront
{
    [Authorize(Roles = "Customer")]
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
        public int VariantId { get; set; }
        public int Quantity { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] bool buynow = false, [FromQuery] int variantid = 0, [FromQuery] int qty = 0)
        {
            IsBuyNow = buynow;
            VariantId = variantid;
            Quantity = qty;

            var client = _httpClientFactory.CreateClient("WebAPI");

            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (IsBuyNow && VariantId > 0 && Quantity > 0)
            {
                var response = await client.GetAsync($"/api/productvariants/{VariantId}");
                Console.WriteLine($"Calling: /api/productvariants/{VariantId}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    var laptopName = doc.RootElement.GetProperty("product").GetProperty("productName").GetString();
                    var price = doc.RootElement.GetProperty("price").GetDecimal();

                    DirectPurchaseItem = new OrderDetailViewModel
                    {
                        VariantId = VariantId,
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
                var response = await client.GetAsync("/api/Cart");
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
