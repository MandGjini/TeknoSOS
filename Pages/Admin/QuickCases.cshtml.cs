using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class QuickCasesModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public QuickCasesModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IList<TeknoSOS.WebApp.Domain.Entities.QuickCase> Cases { get; set; } = new List<TeknoSOS.WebApp.Domain.Entities.QuickCase>();

        public async Task OnGetAsync()
        {
            Cases = await _db.QuickCases.OrderByDescending(q => q.CreatedDate).Take(200).ToListAsync();
        }

        public async Task<IActionResult> OnPostMarkReviewedAsync(int id)
        {
            var qc = await _db.QuickCases.FindAsync(id);
            if (qc == null) return NotFound();
            qc.IsReviewed = true;
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var qc = await _db.QuickCases.FindAsync(id);
            if (qc == null) return NotFound();
            _db.QuickCases.Remove(qc);
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
