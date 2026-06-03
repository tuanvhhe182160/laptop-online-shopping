using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToPage("/Storefront/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Xóa Cookie đăng nhập
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToPage("/Storefront/Index");
        }
    }
}
