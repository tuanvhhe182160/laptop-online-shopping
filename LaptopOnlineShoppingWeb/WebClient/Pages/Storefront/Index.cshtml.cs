using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Entities;

namespace WebClient.Pages.Storefront
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IList<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public IList<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        public string? ApiBaseUrl { get; private set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            ApiBaseUrl = client.BaseAddress?.ToString().TrimEnd('/');

            // 1. Fetch Categories for Filter Dropdown
            try
            {
                var catResponse = await client.GetAsync("api/Categories");
                if (catResponse.IsSuccessStatusCode)
                {
                    var catData = await catResponse.Content.ReadAsStringAsync();
                    Categories = JsonSerializer.Deserialize<List<Category>>(catData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Category>();
                }
            }
            catch (Exception)
            {
            }

            // 2. Fetch ProductVariants with OData query filters
            try
            {
                // We need PhysicalProducts to count stock, and Product -> Category
                var odataQuery = "api/ProductVariants?$expand=PhysicalProducts,Product($expand=Category)";

                var filters = new List<string>();

                // Active products only
                filters.Add("Product/Status eq true");

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var escSearch = Search.Replace("'", "''");
                    filters.Add($"(contains(tolower(Product/ProductName), tolower('{escSearch}')) or contains(tolower(Product/ProductCode), tolower('{escSearch}')) or contains(tolower(Cpu), tolower('{escSearch}')))");
                }

                if (CategoryId.HasValue && CategoryId.Value > 0)
                {
                    filters.Add($"Product/CategoryId eq {CategoryId.Value}");
                }

                if (MinPrice.HasValue && MinPrice.Value > 0)
                {
                    filters.Add($"Price ge {MinPrice.Value}");
                }

                if (MaxPrice.HasValue && MaxPrice.Value > 0)
                {
                    filters.Add($"Price le {MaxPrice.Value}");
                }

                if (filters.Count > 0)
                {
                    odataQuery += "&$filter=" + string.Join(" and ", filters);
                }

                var response = await client.GetAsync(odataQuery);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    
                    using (var doc = JsonDocument.Parse(data))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("value", out var valueElement))
                        {
                            ProductVariants = JsonSerializer.Deserialize<List<ProductVariant>>(valueElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProductVariant>();
                        }
                        else
                        {
                            ProductVariants = JsonSerializer.Deserialize<List<ProductVariant>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ProductVariant>();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
