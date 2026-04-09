using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class HowItWorksModel : PageModel
    {
        private readonly ISiteContentService _content;

        public HowItWorksModel(ISiteContentService content)
        {
            _content = content;
        }

        public List<SiteContent> CitizenSteps { get; set; } = new();
        public List<SiteContent> ProfessionalSteps { get; set; } = new();
        public List<SiteContent> Tips { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allSteps = await _content.GetPageContentByTypeAsync("howitworks", "step");
            CitizenSteps = allSteps.Where(s => s.SectionKey.StartsWith("citizen_step_")).OrderBy(s => s.SortOrder).ToList();
            ProfessionalSteps = allSteps.Where(s => s.SectionKey.StartsWith("pro_step_")).OrderBy(s => s.SortOrder).ToList();
            Tips = await _content.GetPageContentByTypeAsync("howitworks", "feature");
        }
    }
}
