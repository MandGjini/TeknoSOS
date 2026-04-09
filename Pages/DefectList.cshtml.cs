using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    [Authorize]
    public class DefectListModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DefectListModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<ServiceRequest> Defects { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isPro = await _userManager.IsInRoleAsync(user, "Professional");

            IQueryable<ServiceRequest> query = _db.ServiceRequests
                .Include(s => s.Citizen)
                .Include(s => s.Professional)
                .OrderByDescending(s => s.CreatedDate);

            // Role-based filtering
            if (isAdmin)
            {
                // Admin sees everything
            }
            else if (isPro)
            {
                query = query.Where(s => s.ProfessionalId == user.Id || s.Status == ServiceRequestStatus.Created);
            }
            else
            {
                query = query.Where(s => s.CitizenId == user.Id);
            }

            // Apply filters
            if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<ServiceRequestStatus>(StatusFilter, out var status))
            {
                query = query.Where(s => s.Status == status);
            }

            if (!string.IsNullOrEmpty(CategoryFilter) && Enum.TryParse<ServiceCategory>(CategoryFilter, out var cat))
            {
                query = query.Where(s => s.Category == cat);
            }

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(s => s.Title.Contains(SearchQuery) || s.Description.Contains(SearchQuery) || (s.Location != null && s.Location.Contains(SearchQuery)));
            }

            Defects = await query.ToListAsync();
        }
    }
}
