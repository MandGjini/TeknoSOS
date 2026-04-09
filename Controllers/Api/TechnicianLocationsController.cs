using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Controllers.Api
{
    /// <summary>
    /// API for technician location data - requires authentication
    /// </summary>
    [ApiController]
    [Route("api/technicians")]
    [Authorize]
    [EnableRateLimiting("api")]
    public class TechnicianLocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TechnicianLocationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all technician locations for the map
        /// </summary>
        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations([FromQuery] string? category = null)
        {
            var query = _db.Users
                .Where(u => u.Role == UserRole.Professional && u.IsActive);

            var technicians = await query
                .Select(u => new
                {
                    id = u.Id,
                    fullName = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                    city = u.City,
                    latitude = u.Latitude,
                    longitude = u.Longitude,
                    isAvailable = u.IsAvailableForWork,
                    isVerified = u.IsProfileVerified,
                    averageRating = u.AverageRating,
                    serviceRadiusKm = u.ServiceRadiusKm,
                    specialties = string.Join(", ", _db.ProfessionalSpecialties
                        .Where(ps => ps.ProfessionalId == u.Id)
                        .Select(ps => ps.Category.ToString()))
                })
                .ToListAsync();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                technicians = technicians
                    .Where(t => t.specialties != null && 
                               t.specialties.Contains(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(technicians);
        }

        /// <summary>
        /// Get nearby technicians based on location
        /// </summary>
        [HttpGet("nearby")]
        public async Task<IActionResult> GetNearbyTechnicians(
            [FromQuery] decimal latitude, 
            [FromQuery] decimal longitude, 
            [FromQuery] int radiusKm = 50,
            [FromQuery] string? category = null)
        {
            var technicians = await _db.Users
                .Where(u => u.Role == UserRole.Professional && 
                           u.IsActive && 
                           u.IsAvailableForWork &&
                           u.Latitude.HasValue && 
                           u.Longitude.HasValue)
                .Select(u => new
                {
                    id = u.Id,
                    fullName = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                    city = u.City,
                    latitude = u.Latitude!.Value,
                    longitude = u.Longitude!.Value,
                    isVerified = u.IsProfileVerified,
                    averageRating = u.AverageRating,
                    serviceRadiusKm = u.ServiceRadiusKm ?? 30,
                    specialties = string.Join(", ", _db.ProfessionalSpecialties
                        .Where(ps => ps.ProfessionalId == u.Id)
                        .Select(ps => ps.Category.ToString())),
                    profileImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();

            // Filter by distance using Haversine formula
            var nearbyTechnicians = technicians
                .Select(t => new
                {
                    t.id,
                    t.fullName,
                    t.city,
                    t.latitude,
                    t.longitude,
                    t.isVerified,
                    t.averageRating,
                    t.serviceRadiusKm,
                    t.specialties,
                    t.profileImageUrl,
                    distance = CalculateDistance(latitude, longitude, t.latitude, t.longitude)
                })
                .Where(t => t.distance <= radiusKm)
                .OrderBy(t => t.distance)
                .ToList();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                nearbyTechnicians = nearbyTechnicians
                    .Where(t => t.specialties != null && 
                               t.specialties.Contains(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(nearbyTechnicians);
        }

        /// <summary>
        /// Update technician location (for real-time tracking)
        /// </summary>
        [HttpPost("update-location")]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationUpdateRequest request)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Professional)
                return NotFound("Technician not found");

            user.Latitude = request.Latitude;
            user.Longitude = request.Longitude;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Location updated" });
        }

        /// <summary>
        /// Calculate distance between two coordinates using Haversine formula
        /// </summary>
        private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Earth's radius in km
            
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c;
        }

        private static double ToRadians(double deg) => deg * Math.PI / 180;

        public class LocationUpdateRequest
        {
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }
    }
}
