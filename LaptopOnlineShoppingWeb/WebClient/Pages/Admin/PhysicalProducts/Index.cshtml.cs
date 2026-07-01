using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages.Admin.PhysicalProducts
{
    [Authorize(Roles = "Admin,WarehouseManager")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
