using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Entities;

namespace WebClient.Pages.Storefront
{
    public class ProductDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductDetailModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ProductVariant? Variant { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            var response = await client.GetAsync($"odata/ProductVariants({id})?$expand=Product($expand=Category),PhysicalProducts");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Variant = JsonSerializer.Deserialize<ProductVariant>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (Variant == null) return NotFound();
                return Page();
            }
            
            return NotFound();
        }
    }
}
