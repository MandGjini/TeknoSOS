using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class AboutModel : PageModel
    {
        private readonly ISiteContentService _content;

        public AboutModel(ISiteContentService content)
        {
            _content = content;
        }

        public List<SiteContent> Values { get; set; } = new();
        public List<SiteContent> Stats { get; set; } = new();
        public SiteContent? FullPageContent { get; set; }
        public bool HasCustomContent => FullPageContent != null && !string.IsNullOrWhiteSpace(FullPageContent.Content);

        public async Task OnGetAsync()
        {
            // Check for custom full page content from Page Editor
            FullPageContent = await _content.GetBlockAsync("about", "full_page");
            
            // Also load individual sections in case they're needed
            Values = await _content.GetPageContentByTypeAsync("about", "feature");
            Stats = await _content.GetPageContentByTypeAsync("about", "stat");
        }
    }
}
