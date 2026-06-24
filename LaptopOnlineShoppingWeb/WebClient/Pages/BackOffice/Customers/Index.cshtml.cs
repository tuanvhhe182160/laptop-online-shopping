using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebClient.Models;

namespace WebClient.Pages.BackOffice.Customers
{
    [Authorize(Roles = "Admin,Staff")]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CustomerViewModel> Customers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");

            // 1. MẶC GIÁP TOKEN
            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 2. Odata query: Lấy danh sách khách hàng, nếu có SearchTerm thì lọc theo FullName hoặc Phone
            string endpoint = "api/customers";

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                string cleanSearch = SearchTerm.Trim().ToLower();
                endpoint += $"?$filter=contains(tolower(FullName), '{cleanSearch}') or contains(Phone, '{cleanSearch}')";
            }

            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Customers = JsonSerializer.Deserialize<List<CustomerViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return Page();
        }
    }
}