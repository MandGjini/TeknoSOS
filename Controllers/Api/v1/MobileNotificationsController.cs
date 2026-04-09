using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Controllers.Api.v1;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
[Produces("application/json")]
public class MobileNotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MobileNotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] bool unreadOnly = false)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Unauthorized" });

        const int pageSize = 20;
        page = page < 1 ? 1 : page;

        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new MobileNotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedDate,
                LinkUrl = n.ServiceRequestId > 0 ? $"/requests/{n.ServiceRequestId}" : null
            })
            .ToListAsync();

        return Ok(new ApiResponse<PagedResult<MobileNotificationDto>>
        {
            Success = true,
            Data = new PagedResult<MobileNotificationDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Unauthorized" });

        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Device token is required" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

        user.DevicePushToken = request.Token.Trim();
        user.DevicePlatform = string.IsNullOrWhiteSpace(request.Platform) ? "Android" : request.Platform.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to register device"
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Device registered"
        });
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Unauthorized" });

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.RecipientId == userId);

        if (notification == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Notification not found" });

        notification.IsRead = true;
        notification.ReadDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Notification marked as read"
        });
    }
}

public class RegisterDeviceRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = "Android";
}

public class MobileNotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LinkUrl { get; set; }
}
