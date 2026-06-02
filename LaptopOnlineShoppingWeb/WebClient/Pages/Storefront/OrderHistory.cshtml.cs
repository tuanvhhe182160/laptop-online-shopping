using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebClient.Models;
using System.Security.Claims;

namespace WebClient.Pages.Storefront
{
    public class OrderHistoryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderHistoryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            var customerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int customerId = int.TryParse(customerIdStr, out int id) ? id : 1; 

            var client = _httpClientFactory.CreateClient("WebAPI");
            var response = await client.GetAsync($"/api/orders/customer/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var list = await JsonSerializer.DeserializeAsync<List<OrderViewModel>>(await response.Content.ReadAsStreamAsync(), options);
                if (list != null) Orders = list;
            }

            return Page();
        }
    }
}
