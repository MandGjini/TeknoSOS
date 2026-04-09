using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Authorization;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    // [Microsoft.AspNetCore.Authorization.Authorize(Users = "your@email.com")] // Uncomment and set your email for user-specific access
    public class BacklinksModel : PageModel
    {
        [BindProperty]
        public string TargetUrl { get; set; } = string.Empty;
        [BindProperty]
        public string LinkTitle { get; set; } = string.Empty;
        [BindProperty]
        public string AnchorText { get; set; } = string.Empty;
        [BindProperty]
        public string RelAttribute { get; set; } = string.Empty;
        public string GeneratedBacklink { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (!string.IsNullOrWhiteSpace(TargetUrl) && !string.IsNullOrWhiteSpace(AnchorText))
            {
                var rel = string.IsNullOrWhiteSpace(RelAttribute) ? "nofollow" : RelAttribute;
                GeneratedBacklink = $"<a href=\"{TargetUrl}\" title=\"{LinkTitle}\" rel=\"{rel}\">{AnchorText}</a>";
            }
        }
    }
}
