using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Controllers.Api.v1;

[ApiController]
[Route("api/v1/technicians")]
[Authorize]
[Produces("application/json")]
public class MobileTechniciansController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MobileTechniciansController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetTechnicians(
        [FromQuery] string? category = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double? radiusKm = null)
    {
        var query = _db.Users
            .Where(u => u.Role == UserRole.Professional && u.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<ServiceCategory>(category, out var parsedCategory))
        {
            query = query.Where(u => _db.ProfessionalSpecialties.Any(ps => ps.ProfessionalId == u.Id && ps.Category == parsedCategory));
        }

        var technicians = await query
            .Select(u => new TechnicianMobileDto
            {
                Id = u.Id,
                DisplayName = !string.IsNullOrWhiteSpace(u.DisplayUsername) ? u.DisplayUsername : ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                ProfileImageUrl = u.ProfileImageUrl,
                Specialties = _db.ProfessionalSpecialties
                    .Where(ps => ps.ProfessionalId == u.Id)
                    .Select(ps => ps.Category.ToString())
                    .ToList(),
                Rating = (double)(u.AverageRating ?? 0),
                ReviewCount = u.TotalReviews,
                City = u.City,
                IsAvailable = u.IsAvailableForWork,
                IsVerified = u.IsProfileVerified,
                Distance = lat.HasValue && lng.HasValue && u.Latitude.HasValue && u.Longitude.HasValue
                    ? CalculateDistanceKm(lat.Value, lng.Value, (double)u.Latitude.Value, (double)u.Longitude.Value)
                    : null
            })
            .ToListAsync();

        if (radiusKm.HasValue)
        {
            technicians = technicians
                .Where(t => !t.Distance.HasValue || t.Distance.Value <= radiusKm.Value)
                .ToList();
        }

        technicians = technicians
            .OrderBy(t => t.Distance ?? double.MaxValue)
            .ThenByDescending(t => t.IsVerified)
            .ThenByDescending(t => t.Rating)
            .ToList();

        return Ok(new ApiResponse<List<TechnicianMobileDto>>
        {
            Success = true,
            Data = technicians
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTechnician(string id)
    {
        var technician = await _db.Users
            .Where(u => u.Id == id && u.Role == UserRole.Professional && u.IsActive)
            .Select(u => new TechnicianMobileDto
            {
                Id = u.Id,
                DisplayName = !string.IsNullOrWhiteSpace(u.DisplayUsername) ? u.DisplayUsername : ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                ProfileImageUrl = u.ProfileImageUrl,
                Specialties = _db.ProfessionalSpecialties
                    .Where(ps => ps.ProfessionalId == u.Id)
                    .Select(ps => ps.Category.ToString())
                    .ToList(),
                Rating = (double)(u.AverageRating ?? 0),
                ReviewCount = u.TotalReviews,
                City = u.City,
                IsAvailable = u.IsAvailableForWork,
                IsVerified = u.IsProfileVerified,
                Distance = null
            })
            .FirstOrDefaultAsync();

        if (technician == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Technician not found" });
        }

        return Ok(new ApiResponse<TechnicianMobileDto>
        {
            Success = true,
            Data = technician
        });
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var originLat = ToRadians(lat1);
        var targetLat = ToRadians(lat2);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(originLat) * Math.Cos(targetLat) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double angle) => angle * Math.PI / 180.0;

    public class TechnicianMobileDto
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public List<string> Specialties { get; set; } = new();
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string? City { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVerified { get; set; }
        public double? Distance { get; set; }
    }
}
