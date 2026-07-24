using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;
using WebClient.Models;

namespace WebClient.Pages.Storefront
{
    //[Authorize(Roles = "Customer")]
    public class CartModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CartModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public CartViewModel? Cart { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("WebAPI");

            // Lấy token từ Cookie
            var token = User.FindFirst("AccessToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync("/api/Cart");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Cart = await JsonSerializer.DeserializeAsync<CartViewModel>(await response.Content.ReadAsStreamAsync(), options);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Nếu gọi API thất bại do Token hết hạn/lỗi, đẩy về trang Login
               return RedirectToPage("/Auth/CustomerLogin");
            }

            return Page();
        }
    }
}
