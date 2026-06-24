using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WebClient.ViewModels.Category;

namespace WebClient.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<CategoryViewModel> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            var response = await client.GetAsync("api/Categories");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }
    }
}
