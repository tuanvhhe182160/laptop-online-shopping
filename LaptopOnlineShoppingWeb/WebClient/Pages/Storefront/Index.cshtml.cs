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

        public IList<Laptop> Laptops { get; set; } = new List<Laptop>();
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
                // Safe ignore or fallback
            }

            // 2. Fetch Laptops with OData query filters
            try
            {
                // Base OData path with Category expansion
                var odataQuery = "odata/Laptops?$expand=Category";

                // Build OData $filter expressions
                var filters = new List<string>();

                // Customers should only see active (selling) laptops
                filters.Add("Status eq true");

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var escSearch = Search.Replace("'", "''"); // escape single quotes
                    filters.Add($"(contains(tolower(LaptopName), tolower('{escSearch}')) or contains(tolower(LaptopCode), tolower('{escSearch}')))");
                }

                if (CategoryId.HasValue && CategoryId.Value > 0)
                {
                    filters.Add($"CategoryId eq {CategoryId.Value}");
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

                // Add sorting (newest laptops first)
                odataQuery += "&$orderby=CreatedDate desc";

                var response = await client.GetAsync(odataQuery);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    
                    // Parse OData wrapped response {"value": [...]}
                    using (var doc = JsonDocument.Parse(data))
                    {
                        if (doc.RootElement.TryGetProperty("value", out var valueElement))
                        {
                            Laptops = JsonSerializer.Deserialize<List<Laptop>>(valueElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Laptop>();
                        }
                        else
                        {
                            Laptops = JsonSerializer.Deserialize<List<Laptop>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Laptop>();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Safe ignore or fallback
            }
        }
    }
}
