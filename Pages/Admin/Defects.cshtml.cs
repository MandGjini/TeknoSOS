using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DefectsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DefectsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<ServiceRequest> AllDefects { get; set; } = new();
        public string? SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"]?.ToString();

            IQueryable<ServiceRequest> query = _db.ServiceRequests
                .Include(s => s.Citizen)
                .Include(s => s.Professional)
                .OrderByDescending(s => s.CreatedDate);

            if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<ServiceRequestStatus>(StatusFilter, out var status))
                query = query.Where(s => s.Status == status);

            if (!string.IsNullOrEmpty(CategoryFilter) && Enum.TryParse<ServiceCategory>(CategoryFilter, out var cat))
                query = query.Where(s => s.Category == cat);

            if (!string.IsNullOrEmpty(SearchQuery))
                query = query.Where(s => s.Title.Contains(SearchQuery) || s.Description.Contains(SearchQuery));

            AllDefects = await query.ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int defectId, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus)) return RedirectToPage();

            var defect = await _db.ServiceRequests.FindAsync(defectId);
            if (defect != null && Enum.TryParse<ServiceRequestStatus>(newStatus, out var status))
            {
                defect.Status = status;
                if (status == ServiceRequestStatus.Completed)
                    defect.CompletedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Statusi i defektit #{defectId} u ndryshua në {newStatus}.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int defectId)
        {
            var defect = await _db.ServiceRequests.FindAsync(defectId);
            if (defect != null)
            {
                _db.ServiceRequests.Remove(defect);
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Defekti #{defectId} u fshi.";
            }
            return RedirectToPage();
        }
    }
}
