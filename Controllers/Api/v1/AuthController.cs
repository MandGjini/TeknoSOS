using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Hubs;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Controllers.Api.v1;

/// <summary>
/// Mobile API Authentication Controller (JWT-based)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IEmailSender _emailSender;
    private readonly INotificationTemplateService _templateService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ApplicationDbContext context,
        ILogger<AuthController> logger,
        IHubContext<NotificationHub> notificationHub,
        IEmailSender emailSender,
        INotificationTemplateService templateService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
        _notificationHub = notificationHub;
        _emailSender = emailSender;
        _templateService = templateService;
    }

    /// <summary>
    /// Login and get JWT tokens
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request" });

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Account locked. Try again later." });
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid credentials" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.LastLoginDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            user.DevicePushToken = request.DeviceToken;
            user.DevicePlatform = string.IsNullOrWhiteSpace(request.DevicePlatform) ? user.DevicePlatform : request.DevicePlatform;
        }
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in via mobile API", user.Email);

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                User = MapToUserDto(user, roles)
            }
        });
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request" });

        // Check if email exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Conflict(new ApiResponse<object> { Success = false, Message = "Email already registered" });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName ?? string.Empty,
            LastName = request.LastName ?? string.Empty,
            DisplayUsername = $"{(request.FirstName ?? "user").ToLower()}_{Guid.NewGuid().ToString("N")[..6]}",
            PhoneNumber = request.PhoneNumber,
            City = request.City,
            Country = request.Country ?? "AL",
            EmailConfirmed = true, // Auto-confirm for mobile
            IsActive = !request.IsProfessional, // Citizens active immediately, Technicians need verification
            IsAvailableForWork = false, // Technicians can't work until verified
            IsProfileVerified = false, // Needs admin verification
            RegistrationDate = DateTime.UtcNow
        };

        try
        {
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new ApiResponse<object> { Success = false, Message = errors });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception during mobile registration for {Email}", request.Email);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Gabim gjatë krijimit të përdoruesit." });
        }

        // Assign role
        var role = request.IsProfessional ? "Professional" : "Citizen";
        await _userManager.AddToRoleAsync(user, role);

        // Generate tokens
        var roles = new List<string> { role };
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("New user {Email} registered via mobile API as {Role}", user.Email, role);

        // Notify admins when a new technician/professional registers
        if (request.IsProfessional)
        {
            await NotifyAdminsNewTechnician(user);
            await SendTechnicianWelcomeEmail(user);
        }
        else
        {
            await SendCitizenWelcomeEmail(user);
        }

        return CreatedAtAction(nameof(GetProfile), new ApiResponse<AuthResponse>
        {
            Success = true,
            Message = "Registration successful",
            Data = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600,
                User = MapToUserDto(user, roles)
            }
        });
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid token" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid or expired refresh token" });

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Ok(new ApiResponse<AuthResponse>
        {
            Success = true,
            Data = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600,
                User = MapToUserDto(user, roles)
            }
        });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Data = MapToUserDto(user, roles)
        });
    }

    /// <summary>
    /// Get a public user profile by id or display username
    /// </summary>
    [HttpGet("profile/{id}")]
    [Authorize]
    public async Task<IActionResult> GetPublicProfile(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid profile id" });

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id || u.DisplayUsername == id);

        if (user == null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

        var roles = await _userManager.GetRolesAsync(user);
        var createdRequestsCount = await _context.ServiceRequests.CountAsync(sr => sr.CitizenId == user.Id);
        var completedRequestsCount = await _context.ServiceRequests.CountAsync(sr => sr.CitizenId == user.Id && sr.Status == ServiceRequestStatus.Completed);
        var writtenReviewsCount = await _context.Reviews.CountAsync(r => r.ReviewerId == user.Id);

        return Ok(new ApiResponse<PublicUserProfileDto>
        {
            Success = true,
            Data = new PublicUserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                DisplayUsername = user.DisplayUsername ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                City = user.City,
                Country = user.Country,
                Address = user.Address,
                PostalCode = user.PostalCode,
                Bio = user.Bio,
                PreferredLanguage = user.PreferredLanguage.ToString(),
                ProfileImageUrl = user.ProfileImageUrl,
                IsVerified = user.IsVerified,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? user.Role.ToString(),
                RegistrationDate = user.RegistrationDate,
                CreatedRequestsCount = createdRequestsCount,
                CompletedRequestsCount = completedRequestsCount,
                WrittenReviewsCount = writtenReviewsCount
            }
        });
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        user.FirstName = request.FirstName ?? user.FirstName;
        user.LastName = request.LastName ?? user.LastName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.City = request.City ?? user.City;
        user.Address = request.Address ?? user.Address;
        user.Bio = request.Bio ?? user.Bio;

        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Profile updated",
            Data = MapToUserDto(user, roles)
        });
    }

    /// <summary>
    /// Logout (invalidate refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _userManager.UpdateAsync(user);
            }
        }
        return Ok(new ApiResponse<object> { Success = true, Message = "Logged out successfully" });
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ApiResponse<object> { Success = false, Message = errors });
        }

        return Ok(new ApiResponse<object> { Success = true, Message = "Password changed successfully" });
    }

    /// <summary>
    /// Notify all admins when a new technician registers
    /// </summary>
    private async Task NotifyAdminsNewTechnician(ApplicationUser newTechnician)
    {
        try
        {
            // Get all admin users
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            
            var title = "Teknik i Ri u Regjistrua!";
            var message = $"{newTechnician.FirstName} {newTechnician.LastName} ({newTechnician.Email}) u regjistrua si teknik i ri. " +
                         $"Qyteti: {newTechnician.City ?? "I pa-specifikuar"}";

            // Create notification for each admin
            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    RecipientId = admin.Id,
                    ServiceRequestId = 0, // No service request associated
                    Type = NotificationType.NewTechnicianRegistered,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                // Send real-time SignalR notification to admin
                await _notificationHub.Clients.Group($"user-{admin.Id}").SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = title,
                    message = message,
                    type = (int)NotificationType.NewTechnicianRegistered,
                    technicianId = newTechnician.Id,
                    technicianName = $"{newTechnician.FirstName} {newTechnician.LastName}",
                    technicianEmail = newTechnician.Email,
                    city = newTechnician.City,
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin notification sent for new technician: {Email}", newTechnician.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify admins about new technician: {Email}", newTechnician.Email);
            // Don't throw - registration should still succeed even if notification fails
        }
    }

    /// <summary>
    /// Send welcome email to new technician explaining the verification process
    /// </summary>
    private async Task SendTechnicianWelcomeEmail(ApplicationUser user)
    {
        try
        {
            var email = user.Email;
            if (string.IsNullOrEmpty(email)) return;

            var placeholders = new Dictionary<string, string>
            {
                { "FirstName", user.FirstName ?? "Teknik" },
                { "LastName", user.LastName ?? "" },
                { "Email", user.Email ?? "" }
            };

            var (subject, body) = await _templateService.RenderEmailTemplateAsync("technician_welcome", placeholders);
            
            if (!string.IsNullOrEmpty(body))
            {
                await _emailSender.SendEmailAsync(email, subject, body);
            }

            _logger.LogInformation("Welcome email sent to new technician: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to technician: {Email}", user.Email);
        }
    }

    /// <summary>
    /// Send welcome email to new citizen
    /// </summary>
    private async Task SendCitizenWelcomeEmail(ApplicationUser user)
    {
        try
        {
            var email = user.Email;
            if (string.IsNullOrEmpty(email)) return;

            var placeholders = new Dictionary<string, string>
            {
                { "FirstName", user.FirstName ?? "Përdorues" },
                { "LastName", user.LastName ?? "" },
                { "Email", user.Email ?? "" }
            };

            var (subject, body) = await _templateService.RenderEmailTemplateAsync("citizen_welcome", placeholders);
            
            if (!string.IsNullOrEmpty(body))
            {
                await _emailSender.SendEmailAsync(email, subject, body);
            }

            _logger.LogInformation("Welcome email sent to new citizen: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to citizen: {Email}", user.Email);
        }
    }

    private static UserDto MapToUserDto(ApplicationUser user, IList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            DisplayUsername = user.DisplayUsername ?? "",
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            Country = user.Country,
            ProfileImageUrl = user.ProfileImageUrl,
            IsVerified = user.IsVerified,
            Role = roles.FirstOrDefault() ?? "Citizen",
            RegistrationDate = user.RegistrationDate
        };
    }
}

#region Request/Response Models

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? DeviceToken { get; set; } // For push notifications
    public string? DevicePlatform { get; set; } // iOS / Android
}

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsProfessional { get; set; }
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Bio { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string DisplayUsername { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsVerified { get; set; }
    public string Role { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
}

public class PublicUserProfileDto : UserDto
{
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? Bio { get; set; }
    public string PreferredLanguage { get; set; } = "English";
    public bool IsActive { get; set; }
    public int CreatedRequestsCount { get; set; }
    public int CompletedRequestsCount { get; set; }
    public int WrittenReviewsCount { get; set; }
}

#endregion
