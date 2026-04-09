using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Controllers.Api.v1;

/// <summary>
/// Mobile-optimized Defects/Service Requests API
/// </summary>
[ApiController]
[Route("api/v1/defects")]
[Authorize]
[Produces("application/json")]
public class MobileDefectsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MobileDefectsController> _logger;
    private readonly IProfessionalOpportunityNotifier _opportunityNotifier;

    public MobileDefectsController(
        ApplicationDbContext context,
        ILogger<MobileDefectsController> logger,
        IProfessionalOpportunityNotifier opportunityNotifier)
    {
        _context = context;
        _logger = logger;
        _opportunityNotifier = opportunityNotifier;
    }

    /// <summary>
    /// Get user's defect list
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyDefects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = _context.ServiceRequests
            .Where(d => d.CitizenId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ServiceRequestStatus>(status, out var statusEnum))
        {
            query = query.Where(d => d.Status == statusEnum);
        }

        var total = await query.CountAsync();
        var defects = await query
            .OrderByDescending(d => d.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DefectListDto
            {
                Id = d.Id,
                TrackingCode = d.UniqueCode,
                Title = d.Title,
                Category = d.Category.ToString(),
                Status = d.Status.ToString(),
                Priority = d.Priority.ToString(),
                Location = d.Location,
                PhotoUrl = d.PhotoUrl,
                CreatedAt = d.CreatedDate,
                HasUnreadMessages = d.Messages.Any(m => m.Status != MessageStatus.Read && m.ReceiverId == userId)
            })
            .ToListAsync();

        return Ok(new ApiResponse<PagedResult<DefectListDto>>
        {
            Success = true,
            Data = new PagedResult<DefectListDto>
            {
                Items = defects,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Get defect details
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDefect(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var defect = await _context.ServiceRequests
            .Include(d => d.Citizen)
            .Include(d => d.Professional)
            .Include(d => d.TechnicianInterests)
                .ThenInclude(ti => ti.Technician)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (defect == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Defect not found" });

        // Check if user has access (citizen owner or interested technician)
        var userRoles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
        var isCitizen = defect.CitizenId == userId;
        var isTechnician = userRoles.Contains("Professional") || userRoles.Contains("Admin");

        if (!isCitizen && !isTechnician)
            return Forbid();

        return Ok(new ApiResponse<DefectDetailDto>
        {
            Success = true,
            Data = new DefectDetailDto
            {
                Id = defect.Id,
                TrackingCode = defect.UniqueCode,
                Title = defect.Title,
                Description = defect.Description,
                Category = defect.Category.ToString(),
                Status = defect.Status.ToString(),
                Priority = defect.Priority.ToString(),
                Location = defect.Location,
                Latitude = defect.ClientLatitude.HasValue ? (double)defect.ClientLatitude.Value : null,
                Longitude = defect.ClientLongitude.HasValue ? (double)defect.ClientLongitude.Value : null,
                PhotoUrl = defect.PhotoUrl,
                CreatedAt = defect.CreatedDate,
                Citizen = new UserSummaryDto
                {
                    Id = defect.Citizen.Id,
                    DisplayName = defect.Citizen.DisplayUsername ?? "User",
                    ProfileImageUrl = defect.Citizen.ProfileImageUrl
                },
                AssignedTechnician = defect.Professional != null ? new UserSummaryDto
                {
                    Id = defect.Professional.Id,
                    DisplayName = defect.Professional.DisplayUsername ?? "Technician",
                    ProfileImageUrl = defect.Professional.ProfileImageUrl
                } : null,
                InterestCount = defect.TechnicianInterests.Count
            }
        });
    }

    /// <summary>
    /// Create a new defect/service request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDefect([FromBody] CreateDefectRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (!Enum.TryParse<ServiceCategory>(request.Category, out var category))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid category" });

        if (!Enum.TryParse<ServiceRequestPriority>(request.Priority, out var priority))
            priority = ServiceRequestPriority.Normal;

        // Generate tracking code
        var lastCode = await _context.ServiceRequests
            .OrderByDescending(s => s.Id)
            .Select(s => s.UniqueCode)
            .FirstOrDefaultAsync();
        
        var nextNumber = 1;
        if (!string.IsNullOrEmpty(lastCode) && lastCode.StartsWith("DEF-"))
        {
            if (int.TryParse(lastCode.Replace("DEF-", ""), out var lastNum))
                nextNumber = lastNum + 1;
        }
        var trackingCode = $"DEF-{nextNumber:D6}";

        var defect = new ServiceRequest
        {
            UniqueCode = trackingCode,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Category = category,
            Priority = priority,
            Status = ServiceRequestStatus.Created,
            Location = request.Location,
            ClientLatitude = request.Latitude.HasValue ? (decimal)request.Latitude.Value : null,
            ClientLongitude = request.Longitude.HasValue ? (decimal)request.Longitude.Value : null,
            PhotoUrl = request.PhotoUrl,
            CitizenId = userId,
            CreatedDate = DateTime.UtcNow
        };

        _context.ServiceRequests.Add(defect);
        await _context.SaveChangesAsync();

        var notifiedCount = await _opportunityNotifier.NotifyNewOpportunityAsync(defect, "Mobile");

        _logger.LogInformation(
            "New defect {TrackingCode} created by {UserId} via mobile. Notified professionals: {NotifiedCount}",
            trackingCode,
            userId,
            notifiedCount);

        return CreatedAtAction(nameof(GetDefect), new { id = defect.Id }, new ApiResponse<DefectDetailDto>
        {
            Success = true,
            Message = "Defect created successfully",
            Data = new DefectDetailDto
            {
                Id = defect.Id,
                TrackingCode = defect.UniqueCode,
                Title = defect.Title,
                Category = defect.Category.ToString(),
                Status = defect.Status.ToString(),
                CreatedAt = defect.CreatedDate
            }
        });
    }

    /// <summary>
    /// Get nearby defects (for technicians)
    /// </summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyDefects(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusKm = 10,
        [FromQuery] string? category = null)
    {
        var query = _context.ServiceRequests
            .Where(d => d.Status == ServiceRequestStatus.Created || d.Status == ServiceRequestStatus.Matched)
            .Where(d => d.ClientLatitude.HasValue && d.ClientLongitude.HasValue)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category) && Enum.TryParse<ServiceCategory>(category, out var cat))
        {
            query = query.Where(d => d.Category == cat);
        }

        // Get all defects and filter by distance in memory (EF Core doesn't support geography functions directly)
        var allDefects = await query.ToListAsync();
        var nearbyDefects = allDefects
            .Where(d => CalculateDistance(lat, lng, (double)d.ClientLatitude!.Value, (double)d.ClientLongitude!.Value) <= radiusKm)
            .OrderBy(d => CalculateDistance(lat, lng, (double)d.ClientLatitude!.Value, (double)d.ClientLongitude!.Value))
            .Take(50)
            .Select(d => new DefectListDto
            {
                Id = d.Id,
                TrackingCode = d.UniqueCode,
                Title = d.Title,
                Category = d.Category.ToString(),
                Status = d.Status.ToString(),
                Priority = d.Priority.ToString(),
                Location = d.Location,
                PhotoUrl = d.PhotoUrl,
                CreatedAt = d.CreatedDate,
                Distance = Math.Round(CalculateDistance(lat, lng, (double)d.ClientLatitude!.Value, (double)d.ClientLongitude!.Value), 2)
            })
            .ToList();

        return Ok(new ApiResponse<List<DefectListDto>>
        {
            Success = true,
            Data = nearbyDefects
        });
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180;
}

#region DTOs

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public class DefectListDto
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string? Location { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasUnreadMessages { get; set; }
    public double? Distance { get; set; }
}

public class DefectDetailDto
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Priority { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserSummaryDto? Citizen { get; set; }
    public UserSummaryDto? AssignedTechnician { get; set; }
    public int InterestCount { get; set; }
}

public class UserSummaryDto
{
    public string Id { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}

public class CreateDefectRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public string? Priority { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PhotoUrl { get; set; }
}

#endregion
