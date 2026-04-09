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
    public class GPSMonitorModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GPSMonitorModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<TechnicianGPS> TechnicianLocations { get; set; } = new();
        public List<DefectGPS> DefectLocations { get; set; } = new();
        public int TotalWithGPS { get; set; }
        public int TotalAvailable { get; set; }
        public int ActiveEmergencies { get; set; }

        public class TechnicianGPS
        {
            public string Id { get; set; } = "";
            public string FullName { get; set; } = "";
            public string? City { get; set; }
            public decimal Lat { get; set; }
            public decimal Lon { get; set; }
            public bool IsAvailable { get; set; }
            public bool IsVerified { get; set; }
            public List<string> Specialties { get; set; } = new();
            public int? ServiceRadius { get; set; }
        }

        public class DefectGPS
        {
            public int Id { get; set; }
            public string UniqueCode { get; set; } = "";
            public string Title { get; set; } = "";
            public string Category { get; set; } = "";
            public string Priority { get; set; } = "";
            public string Status { get; set; } = "";
            public decimal Lat { get; set; }
            public decimal Lon { get; set; }
            public string? Location { get; set; }
            public string ClientName { get; set; } = "";
            public DateTime CreatedDate { get; set; }
        }

        public async Task OnGetAsync()
        {
            // Technicians with GPS
            TechnicianLocations = await _db.Users
                .Where(u => u.Role == UserRole.Professional && u.IsActive && u.Latitude != null && u.Longitude != null)
                .Select(u => new TechnicianGPS
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    City = u.City,
                    Lat = u.Latitude!.Value,
                    Lon = u.Longitude!.Value,
                    IsAvailable = u.IsAvailableForWork,
                    IsVerified = u.IsProfileVerified,
                    Specialties = u.Specialties.Select(s => s.Category.ToString()).ToList(),
                    ServiceRadius = u.ServiceRadiusKm
                })
                .ToListAsync();

            TotalWithGPS = TechnicianLocations.Count;
            TotalAvailable = TechnicianLocations.Count(t => t.IsAvailable);

            // Active defects with GPS
            DefectLocations = await _db.ServiceRequests
                .Include(d => d.Citizen)
                .Where(d => d.ClientLatitude != null && d.ClientLongitude != null &&
                            d.Status != ServiceRequestStatus.Completed &&
                            d.Status != ServiceRequestStatus.Cancelled)
                .Select(d => new DefectGPS
                {
                    Id = d.Id,
                    UniqueCode = d.UniqueCode,
                    Title = d.Title,
                    Category = d.Category.ToString(),
                    Priority = d.Priority.ToString(),
                    Status = d.Status.ToString(),
                    Lat = d.ClientLatitude!.Value,
                    Lon = d.ClientLongitude!.Value,
                    Location = d.Location,
                    ClientName = d.Citizen.FirstName + " " + d.Citizen.LastName,
                    CreatedDate = d.CreatedDate
                })
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            ActiveEmergencies = DefectLocations.Count(d => d.Priority == "Emergency");
        }
    }
}
