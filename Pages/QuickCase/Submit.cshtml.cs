using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.QuickCase
{
    public class SubmitModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public SubmitModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public class InputModel
        {
            [StringLength(200)]
            public string Name { get; set; } = string.Empty;

            [EmailAddress]
            [StringLength(200)]
            public string? Email { get; set; }

            [StringLength(50)]
            public string? Phone { get; set; }

            [Required]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            [StringLength(4000)]
            public string Description { get; set; } = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var qc = new TeknoSOS.WebApp.Domain.Entities.QuickCase
            {
                Name = Input.Name?.Trim() ?? string.Empty,
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim(),
                Title = Input.Title.Trim(),
                Description = Input.Description.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            _db.QuickCases.Add(qc);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Faleminderit — rasti u dërgua. Admini do ta shikojë në panel.";

            return RedirectToPage();
        }
    }
}
