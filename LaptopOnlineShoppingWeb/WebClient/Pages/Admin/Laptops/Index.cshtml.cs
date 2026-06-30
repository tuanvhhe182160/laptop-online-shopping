using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.Entities;

namespace WebClient.Pages.Adminc.ProductVariants
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IList<Laptop> Laptops { get; set; } = new List<Laptop>();
        public IList<Category> Categories { get; set; } = new List<Category>();
        public string AccessToken { get; set; } = string.Empty;
        public string ApiBaseUrl { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? Status { get; set; }

        public async Task OnGetAsync()
        {
            // Retrieve token from logged-in user claims
            AccessToken = User.FindFirst("AccessToken")?.Value ?? string.Empty;

            var client = _httpClientFactory.CreateClient("WebAPI");
            ApiBaseUrl = client.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;

            // 1. Fetch Categories for dropdown list
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
                // Fallback
            }

            // 2. Fetch Laptops for the list (using OData for expansion and filtering)
            try
            {
                var odataQuery = "odata/Laptops?$expand=Category";
                var filters = new List<string>();

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var escSearch = Search.Replace("'", "''");
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

                if (Status.HasValue)
                {
                    filters.Add($"Status eq {Status.Value.ToString().ToLower()}");
                }

                if (filters.Count > 0)
                {
                    odataQuery += "&$filter=" + string.Join(" and ", filters);
                }

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
                // Fallback
            }
        }
    }
}
