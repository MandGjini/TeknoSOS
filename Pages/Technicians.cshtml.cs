using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class TechniciansModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILocalizationService _localizer;

        public TechniciansModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILocalizationService localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        public List<TechnicianCard> TechnicianList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? City { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        public List<string> AvailableCities { get; set; } = new();

        public class TechnicianCard
        {
            public string Id { get; set; } = string.Empty;
            public string? DisplayUsername { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string? ProfileImage { get; set; }
            public string? Bio { get; set; }
            public string? City { get; set; }
            public string? CompanyName { get; set; }
            public decimal? AverageRating { get; set; }
            public int TotalReviews { get; set; }
            public int CompletedJobs { get; set; }
            public int? YearsOfExperience { get; set; }
            public bool IsAvailable { get; set; }
            public bool IsVerified { get; set; }
            public List<string> Specialties { get; set; } = new();
            public int PortfolioCount { get; set; }
        }

        public async Task OnGetAsync()
        {
            var proUsers = await _userManager.GetUsersInRoleAsync("Professional");
            var proIds = proUsers.Select(u => u.Id).ToList();

            var query = _context.Users
                .Where(u => proIds.Contains(u.Id) && u.IsActive
                    && (u.SubscriptionEndDate > DateTime.UtcNow 
                        || (!u.SubscriptionEndDate.HasValue && u.RegistrationDate.AddDays(30) > DateTime.UtcNow)));

            var techData = await query
                .Select(u => new TechnicianCard
                {
                    Id = u.Id,
                    DisplayUsername = u.DisplayUsername,
                    FullName = u.FirstName + " " + u.LastName,
                    ProfileImage = u.ProfileImageUrl,
                    Bio = u.Bio,
                    City = u.City,
                    CompanyName = u.CompanyName,
                    AverageRating = u.AverageRating,
                    TotalReviews = u.TotalReviews,
                    CompletedJobs = u.CompletedJobsCount,
                    YearsOfExperience = u.YearsOfExperience,
                    IsAvailable = u.IsAvailableForWork,
                    IsVerified = u.IsProfileVerified,
                    Specialties = u.Specialties.Select(s => s.Category.ToString()).ToList(),
                    PortfolioCount = u.PortfolioItems.Count
                })
                .ToListAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(Category))
                techData = techData.Where(t => t.Specialties.Any(s => s.Equals(Category, StringComparison.OrdinalIgnoreCase))).ToList();

            if (!string.IsNullOrEmpty(City))
                techData = techData.Where(t => t.City?.Equals(City, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

            if (!string.IsNullOrEmpty(SearchTerm))
                techData = techData.Where(t => t.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.Bio?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            // Sort
            TechnicianList = SortBy switch
            {
                "rating" => techData.OrderByDescending(t => t.AverageRating ?? 0).ToList(),
                "jobs" => techData.OrderByDescending(t => t.CompletedJobs).ToList(),
                "experience" => techData.OrderByDescending(t => t.YearsOfExperience ?? 0).ToList(),
                _ => techData.OrderByDescending(t => t.AverageRating ?? 0).ToList()
            };

            AvailableCities = techData
                .Where(t => !string.IsNullOrEmpty(t.City))
                .Select(t => t.City!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
}
