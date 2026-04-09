using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Models;
using TeknoSOS.WebApp.Data;
using System.Threading.Tasks;
using System.Linq;

namespace TeknoSOS.WebApp.Pages.Admin
{
    public class EditKarikuesEVModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditKarikuesEVModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public new KarikuesEVPageContent Content { get; set; } = new();

        public IActionResult OnGet()
        {
            Content = _db.KarikuesEVPageContents.FirstOrDefault() ?? new KarikuesEVPageContent();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var entity = _db.KarikuesEVPageContents.FirstOrDefault();
            if (entity == null)
            {
                _db.KarikuesEVPageContents.Add(Content);
            }
            else
            {
                entity.HeroTitle = Content.HeroTitle;
                entity.HeroLead = Content.HeroLead;
                // ...shto fushat e tjera sipas modelit
            }
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}