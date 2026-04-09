using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    public class TechniciansMapModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public TechniciansMapModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<TechnicianLocationDto> Technicians { get; set; } = new();
        public int TotalTechnicians { get; set; }
        public int AvailableTechnicians { get; set; }
        public int TechniciansWithGPS { get; set; }
        public List<string> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Get all verified technicians with location data
            var technicians = await _db.Users
                .Where(u => u.Role == UserRole.Professional && u.IsActive)
                .Select(u => new TechnicianLocationDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    City = u.City,
                    Latitude = u.Latitude,
                    Longitude = u.Longitude,
                    IsAvailable = u.IsAvailableForWork,
                    IsVerified = u.IsProfileVerified,
                    AverageRating = u.AverageRating,
                    ServiceRadiusKm = u.ServiceRadiusKm,
                    Specialties = string.Join(", ", _db.ProfessionalSpecialties
                        .Where(ps => ps.ProfessionalId == u.Id)
                        .Select(ps => ps.Category.ToString()))
                })
                .ToListAsync();

            Technicians = technicians;
            TotalTechnicians = technicians.Count;
            AvailableTechnicians = technicians.Count(t => t.IsAvailable);
            TechniciansWithGPS = technicians.Count(t => t.Latitude.HasValue && t.Longitude.HasValue);

            // Get unique categories from ServiceCategory enum
            Categories = Enum.GetNames(typeof(ServiceCategory)).ToList();
        }

        public class TechnicianLocationDto
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string? City { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public bool IsAvailable { get; set; }
            public bool IsVerified { get; set; }
            public decimal? AverageRating { get; set; }
            public int? ServiceRadiusKm { get; set; }
            public string? Specialties { get; set; }
        }
    }
}
