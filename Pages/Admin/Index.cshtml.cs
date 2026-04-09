using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── KPI Cards ──
        public int TotalUsers { get; set; }
        public int TotalCitizens { get; set; }
        public int TotalTechnicians { get; set; }
        public int ActiveTechnicians { get; set; }
        public int ActiveDefects { get; set; }
        public int CompletedDefects { get; set; }
        public int TotalDefects { get; set; }
        public int EmergencyDefects { get; set; }
        public int TotalReviews { get; set; }
        public int TotalMessages { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalRevenue { get; set; }

        // ── Chart Data ──
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
        public Dictionary<string, int> DefectsByCity { get; set; } = new();
        public Dictionary<string, int> DefectsLast7Days { get; set; } = new();

        // ── Tables ──
        public List<ServiceRequest> RecentDefects { get; set; } = new();
        public List<ServiceRequest> EmergencyDefectList { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();
        public List<TechnicianLocationDto> TechnicianLocations { get; set; } = new();

        public class TechnicianLocationDto
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal? Lat { get; set; }
            public decimal? Lon { get; set; }
            public bool IsAvailable { get; set; }
            public string? City { get; set; }
        }

        public async Task OnGetAsync()
        {
            // ── Users (server-side counts) ──
            TotalUsers = await _userManager.Users.CountAsync();
            TotalCitizens = await _userManager.Users.CountAsync(u => u.Role == UserRole.Citizen);
            TotalTechnicians = await _userManager.Users.CountAsync(u => u.Role == UserRole.Professional);
            ActiveTechnicians = await _userManager.Users.CountAsync(u => u.Role == UserRole.Professional && u.IsActive && u.IsAvailableForWork);

            // ── Defects (server-side counts) ──
            TotalDefects = await _db.ServiceRequests.CountAsync();
            ActiveDefects = await _db.ServiceRequests.CountAsync(d =>
                d.Status == ServiceRequestStatus.Created ||
                d.Status == ServiceRequestStatus.Matched ||
                d.Status == ServiceRequestStatus.InProgress);
            CompletedDefects = await _db.ServiceRequests.CountAsync(d => d.Status == ServiceRequestStatus.Completed);
            EmergencyDefects = await _db.ServiceRequests.CountAsync(d =>
                d.Priority == ServiceRequestPriority.Emergency &&
                d.Status != ServiceRequestStatus.Completed &&
                d.Status != ServiceRequestStatus.Cancelled);

            // ── Messages & Reviews ──
            TotalReviews = await _db.Reviews.CountAsync();
            TotalMessages = await _db.Messages.CountAsync();

            // ── Payments ──
            PendingPayments = await _db.TechnicianSubscriptions.CountAsync(s => !s.IsConfirmed && !s.IsTrialPeriod);
            TotalRevenue = await _db.TechnicianSubscriptions
                .Where(s => s.IsConfirmed)
                .SumAsync(s => s.Amount);

            // ── Status Distribution (server-side) ──
            StatusDistribution = await _db.ServiceRequests
                .GroupBy(d => d.Status)
                .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            // ── Category Distribution (server-side) ──
            CategoryDistribution = await _db.ServiceRequests
                .GroupBy(d => d.Category)
                .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            // ── Defects by City (server-side) ──
            DefectsByCity = await _db.ServiceRequests
                .Where(d => d.Location != null && d.Location != "")
                .GroupBy(d => d.Location!)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new { Key = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count);

            // ── Defects Last 7 Days ──
            var last7 = DateTime.UtcNow.AddDays(-7).Date;
            var recentDefects = await _db.ServiceRequests
                .Where(d => d.CreatedDate >= last7)
                .GroupBy(d => d.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();
            DefectsLast7Days = Enumerable.Range(0, 7)
                .Select(i => last7.AddDays(i))
                .ToDictionary(
                    d => d.ToString("dd/MM"),
                    d => recentDefects.FirstOrDefault(x => x.Date == d)?.Count ?? 0);

            // ── Recent Defects ──
            RecentDefects = await _db.ServiceRequests
                .Include(d => d.Citizen)
                .OrderByDescending(d => d.CreatedDate)
                .Take(8)
                .ToListAsync();

            // ── Emergency Defects ──
            EmergencyDefectList = await _db.ServiceRequests
                .Include(d => d.Citizen)
                .Where(d => d.Priority == ServiceRequestPriority.Emergency &&
                            d.Status != ServiceRequestStatus.Completed &&
                            d.Status != ServiceRequestStatus.Cancelled)
                .OrderByDescending(d => d.CreatedDate)
                .Take(5)
                .ToListAsync();

            // ── Recent Users ──
            RecentUsers = await _userManager.Users
                .OrderByDescending(u => u.RegistrationDate)
                .Take(5)
                .ToListAsync();

            // ── Technician GPS Locations ──
            TechnicianLocations = await _db.Users
                .Where(u => u.Role == UserRole.Professional && u.IsActive && u.Latitude != null && u.Longitude != null)
                .Select(u => new TechnicianLocationDto
                {
                    Id = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Lat = u.Latitude,
                    Lon = u.Longitude,
                    IsAvailable = u.IsAvailableForWork,
                    City = u.City
                })
                .ToListAsync();
        }
    }
}
