using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Controllers.Api
{
    /// <summary>
    /// API for defect/service request data
    /// </summary>
    [ApiController]
    [Route("api/defects")]
    [Authorize]
    [EnableRateLimiting("api")]
    public class DefectsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DefectsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get active defect locations for the map
        /// Only shows defects that are still open (Created or Matched status)
        /// </summary>
        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations([FromQuery] string? category = null)
        {
            var activeStatuses = new[] { 
                ServiceRequestStatus.Created, 
                ServiceRequestStatus.Matched 
            };

            var query = _db.ServiceRequests
                .Where(sr => activeStatuses.Contains(sr.Status) && 
                            sr.ClientLatitude.HasValue && 
                            sr.ClientLongitude.HasValue);

            var defects = await query
                .Select(sr => new
                {
                    id = sr.Id,
                    title = sr.Title,
                    location = sr.Location,
                    latitude = sr.ClientLatitude,
                    longitude = sr.ClientLongitude,
                    category = sr.Category.ToString(),
                    status = GetStatusText(sr.Status),
                    priority = sr.Priority.ToString(),
                    createdDate = sr.CreatedDate
                })
                .ToListAsync();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                defects = defects
                    .Where(d => d.category == category)
                    .ToList();
            }

            return Ok(defects);
        }

        /// <summary>
        /// Get defect statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _db.ServiceRequests
                .GroupBy(sr => sr.Status)
                .Select(g => new { status = g.Key.ToString(), count = g.Count() })
                .ToListAsync();

            var categoryStats = await _db.ServiceRequests
                .GroupBy(sr => sr.Category)
                .Select(g => new { category = g.Key.ToString(), count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                byStatus = stats,
                byCategory = categoryStats,
                total = await _db.ServiceRequests.CountAsync()
            });
        }

        private static string GetStatusText(ServiceRequestStatus status) => status switch
        {
            ServiceRequestStatus.Created => "Krijuar",
            ServiceRequestStatus.Matched => "Caktuar",
            ServiceRequestStatus.InProgress => "Në Progres",
            ServiceRequestStatus.Completed => "Përfunduar",
            ServiceRequestStatus.Cancelled => "Anuluar",
            ServiceRequestStatus.Rejected => "Refuzuar",
            _ => status.ToString()
        };
    }
}
