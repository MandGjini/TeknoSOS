using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ISiteContentService _content;

        public IndexModel(ISiteContentService content)
        {
            _content = content;
        }

        public List<SiteContent> Categories { get; set; } = new();
        public List<SiteContent> WhyUs { get; set; } = new();
        public List<SiteContent> Steps { get; set; } = new();
        public List<SiteContent> Stats { get; set; } = new();
        public List<SiteContent> Testimonials { get; set; } = new();
        public List<SiteContent> ProBenefits { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? page = null)
        {
            if (!string.IsNullOrWhiteSpace(page) &&
                Url.IsLocalUrl(page) &&
                page.StartsWith("/Admin/", StringComparison.OrdinalIgnoreCase))
            {
                return LocalRedirect(page);
            }

            var allContent = await _content.GetPageContentAsync("home");
            Categories = allContent.Where(c => c.SectionKey.StartsWith("category_") && c.ContentType == "feature").ToList();
            WhyUs = allContent.Where(c => c.SectionKey.StartsWith("why_") && c.ContentType == "feature").ToList();
            Steps = allContent.Where(c => c.ContentType == "step").OrderBy(c => c.SortOrder).ToList();
            Stats = allContent.Where(c => c.ContentType == "stat").OrderBy(c => c.SortOrder).ToList();
            Testimonials = allContent.Where(c => c.ContentType == "testimonial").OrderBy(c => c.SortOrder).ToList();
            ProBenefits = allContent.Where(c => c.SectionKey.StartsWith("pro_benefit_")).ToList();

            return Page();
        }
    }
}
