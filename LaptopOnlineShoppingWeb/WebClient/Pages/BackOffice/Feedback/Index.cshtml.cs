using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages.BackOffice.Feedback
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
