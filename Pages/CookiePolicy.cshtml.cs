using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class CookiePolicyModel : PageModel
    {
        private readonly ISiteContentService _content;

        public CookiePolicyModel(ISiteContentService content)
        {
            _content = content;
        }

        public List<SiteContent> Sections { get; set; } = new();

        public async Task OnGetAsync()
        {
            Sections = await _content.GetPageContentByTypeAsync("cookie", "html");
        }
    }
}
