using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Entities;

namespace WebClient.Pages.BackOffice.Warehouse
{
    [Authorize(Roles = "WarehouseManager")]
    public class ImportModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ImportModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IList<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");

            try
            {
                // Fetch variants with Product details for dropdown
                var response = await client.GetAsync("odata/ProductVariants?$expand=Product");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(content))
                    {
                        if (doc.RootElement.TryGetProperty("value", out var valueElement))
                        {
                            Variants = JsonSerializer.Deserialize<List<ProductVariant>>(valueElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProductVariant>();
                        }
                        else
                        {
                            Variants = JsonSerializer.Deserialize<List<ProductVariant>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProductVariant>();
                        }
                    }
                }
            }
            catch
            {
                // Ignore gracefully for UI to load empty
            }
        }
    }
}
