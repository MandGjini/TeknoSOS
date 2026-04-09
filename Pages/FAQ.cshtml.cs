using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class FAQModel : PageModel
    {
        private readonly ISiteContentService _content;

        public FAQModel(ISiteContentService content)
        {
            _content = content;
        }

        public List<SiteContent> FAQItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            FAQItems = await _content.GetPageContentByTypeAsync("faq", "faq");
        }
    }
}
