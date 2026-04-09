using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ILogger<ContactModel> _logger;
        private readonly ISiteContentService _content;

        public ContactModel(ILogger<ContactModel> logger, ISiteContentService content)
        {
            _logger = logger;
            _content = content;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public List<SiteContent> InfoItems { get; set; } = new();

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(200)]
            public string Subject { get; set; } = string.Empty;

            [Required]
            [StringLength(2000)]
            public string Message { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            InfoItems = await _content.GetPageContentByTypeAsync("contact", "feature");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            InfoItems = await _content.GetPageContentByTypeAsync("contact", "feature");

            if (!ModelState.IsValid) return Page();

            _logger.LogInformation("Contact form submitted: {Name} <{Email}> - {Subject}",
                Input.Name, Input.Email, Input.Subject);

            SuccessMessage = "Mesazhi u dergua me sukses! Do t'ju kontaktojme se shpejti.";
            return RedirectToPage();
        }
    }
}
