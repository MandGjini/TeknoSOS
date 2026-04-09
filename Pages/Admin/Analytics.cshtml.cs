using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsModel : PageModel
    {
        public int UniqueVisitors { get; set; }
        public int NewRegistrations { get; set; }

        public void OnGet()
        {
            // TODO: Fetch analytics data from DB or external service
            UniqueVisitors = 1234;
            NewRegistrations = 56;
        }
    }
}
