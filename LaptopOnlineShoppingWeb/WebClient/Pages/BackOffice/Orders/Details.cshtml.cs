using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Pages.BackOffice.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public OrderWithDetailsViewModel? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("WebAPI");
            var response = await client.GetAsync($"/api/orders/{id}/details");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Order = await JsonSerializer.DeserializeAsync<OrderWithDetailsViewModel>(await response.Content.ReadAsStreamAsync(), options);
            }

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
