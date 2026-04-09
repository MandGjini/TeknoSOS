using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages
{
    public class BusinessDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BusinessDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Business? Business { get; set; }
        public IEnumerable<BusinessProduct> Products { get; set; } = new List<BusinessProduct>();
        public IEnumerable<BusinessReview> Reviews { get; set; } = new List<BusinessReview>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Business = await _context.Businesses
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

            if (Business == null)
            {
                return Page();
            }

            Products = await _context.BusinessProducts
                .Where(p => p.BusinessId == id && p.IsAvailable)
                .OrderBy(p => p.Name)
                .ToListAsync();

            Reviews = await _context.BusinessReviews
                .Include(r => r.Reviewer)
                .Where(r => r.BusinessId == id)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return Page();
        }
    }
}
