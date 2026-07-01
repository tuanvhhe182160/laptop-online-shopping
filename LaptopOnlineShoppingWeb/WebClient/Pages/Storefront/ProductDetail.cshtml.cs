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
            var response = await client.GetAsync($"api/ProductVariants?$filter=VariantId eq {id}&$expand=Product($expand=Category),PhysicalProducts");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                using (var doc = JsonDocument.Parse(content))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        var firstElement = doc.RootElement[0].GetRawText();
                        Variant = JsonSerializer.Deserialize<ProductVariant>(firstElement, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (Variant != null) return Page();
                    }
                }
            }
            
            return NotFound();
        }
    }
}
