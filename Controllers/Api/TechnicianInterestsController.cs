using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Hubs;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TechnicianInterestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ISiteSettingsService _siteSettings;

        public TechnicianInterestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHubContext<NotificationHub> notificationHub,
            ISiteSettingsService siteSettings)
        {
            _context = context;
            _userManager = userManager;
            _notificationHub = notificationHub;
            _siteSettings = siteSettings;
        }

        /// <summary>
        /// Express interest in a service request (Professional role).
        /// </summary>
        [HttpPost("{serviceRequestId}")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> ExpressInterest(int serviceRequestId, [FromBody] ExpressInterestRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verify service request exists and is open
            var serviceRequest = await _context.ServiceRequests.FindAsync(serviceRequestId);
            if (serviceRequest == null)
                return NotFound(new { error = "Service request not found." });

            if (serviceRequest.Status != ServiceRequestStatus.Created)
                return BadRequest(new { error = "This service request is no longer accepting interests." });

            // Check if already expressed interest
            var existingInterest = await _context.TechnicianInterests
                .AnyAsync(ti => ti.TechnicianId == userId && ti.ServiceRequestId == serviceRequestId &&
                    ti.Status == InterestStatus.Interested);

            if (existingInterest)
                return Conflict(new { error = "You have already expressed interest in this request." });

            var paymentSettings = await _siteSettings.GetGroupAsync("payments");
            var monthlyLimit = paymentSettings.TryGetValue("MonthlyQuoteLimit", out var rawLimit) && int.TryParse(rawLimit, out var parsedLimit)
                ? parsedLimit
                : 20;

            var unlimitedTechnicianIds = paymentSettings.TryGetValue("UnlimitedTechnicianIds", out var rawUnlimited)
                ? rawUnlimited
                : string.Empty;

            var unlimitedSet = unlimitedTechnicianIds
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var isUnlimited = unlimitedSet.Contains(userId);
            if (!isUnlimited)
            {
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var monthlyUsage = await _context.TechnicianInterests
                    .CountAsync(ti => ti.TechnicianId == userId && ti.CreatedDate >= monthStart && ti.CreatedDate < monthEnd);

                if (monthlyUsage >= monthlyLimit)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        error = $"Keni arritur kufirin prej {monthlyLimit} preventivash per kete muaj. Per versionin pa limit, kontaktoni administratorin."
                    });
                }
            }

            var interest = new TechnicianInterest
            {
                TechnicianId = userId,
                ServiceRequestId = serviceRequestId,
                Status = InterestStatus.Interested,
                PreventiveOffer = request.PreventiveOffer?.Trim(),
                EstimatedCost = request.EstimatedCost,
                EstimatedTimeInHours = request.EstimatedTimeInHours,
                CreatedDate = DateTime.UtcNow
            };

            _context.TechnicianInterests.Add(interest);

            // Notify the citizen who created the request
            var techUser = await _userManager.FindByIdAsync(userId);
            _context.Notifications.Add(new Notification
            {
                RecipientId = serviceRequest.CitizenId,
                ServiceRequestId = serviceRequestId,
                Title = "Interes i ri nga tekniku",
                Message = $"{techUser?.GetFullName() ?? "Një teknik"} shprehu interes për kërkesën tuaj.",
                Type = NotificationType.NewInterest,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Push real-time notification to citizen
            await _notificationHub.Clients.Group($"user-{serviceRequest.CitizenId}").SendAsync("ReceiveNotification", new
            {
                title = "Interes i ri nga tekniku",
                message = $"{techUser?.GetFullName() ?? "Një teknik"} shprehu interes për kërkesën tuaj.",
                serviceRequestId = serviceRequestId,
                type = (int)NotificationType.NewInterest
            });

            return Ok(new
            {
                interest.Id,
                interest.TechnicianId,
                interest.ServiceRequestId,
                interest.Status,
                interest.EstimatedCost,
                interest.EstimatedTimeInHours,
                interest.CreatedDate
            });
        }

        /// <summary>
        /// Withdraw interest from a service request.
        /// </summary>
        [HttpDelete("{interestId}/withdraw")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> WithdrawInterest(int interestId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var interest = await _context.TechnicianInterests
                .FirstOrDefaultAsync(ti => ti.Id == interestId && ti.TechnicianId == userId);

            if (interest == null)
                return NotFound(new { error = "Interest not found." });

            if (interest.Status != InterestStatus.Interested)
                return BadRequest(new { error = "Cannot withdraw - interest has already been processed." });

            interest.Status = InterestStatus.Withdrawn;
            interest.ResponseDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Interest withdrawn successfully." });
        }

        /// <summary>
        /// Get all interests for a service request (for request owner or admin).
        /// </summary>
        [HttpGet("request/{serviceRequestId}")]
        public async Task<IActionResult> GetInterestsForRequest(int serviceRequestId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            // Verify ownership or admin
            var serviceRequest = await _context.ServiceRequests.FindAsync(serviceRequestId);
            if (serviceRequest == null)
                return NotFound();

            if (!isAdmin && serviceRequest.CitizenId != userId)
                return Forbid();

            var interests = await _context.TechnicianInterests
                .Where(ti => ti.ServiceRequestId == serviceRequestId)
                .OrderByDescending(ti => ti.CreatedDate)
                .Select(ti => new
                {
                    ti.Id,
                    ti.TechnicianId,
                    TechnicianName = ti.Technician.FirstName + " " + ti.Technician.LastName,
                    ti.Status,
                    ti.PreventiveOffer,
                    ti.EstimatedCost,
                    ti.EstimatedTimeInHours,
                    ti.CreatedDate
                })
                .ToListAsync();

            return Ok(interests);
        }

        /// <summary>
        /// Select a technician (accept their bid) - for request owner or admin.
        /// </summary>
        [HttpPost("{interestId}/select")]
        public async Task<IActionResult> SelectTechnician(int interestId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            var interest = await _context.TechnicianInterests
                .Include(ti => ti.ServiceRequest)
                .Include(ti => ti.Technician)
                .FirstOrDefaultAsync(ti => ti.Id == interestId);

            if (interest == null)
                return NotFound(new { error = "Interest not found." });

            var serviceRequest = interest.ServiceRequest;
            if (serviceRequest == null)
                return NotFound(new { error = "Service request not found." });

            // Only citizen owner or admin can select
            if (!isAdmin && serviceRequest.CitizenId != userId)
                return Forbid();

            if (serviceRequest.Status != ServiceRequestStatus.Created)
                return BadRequest(new { error = "This request is no longer accepting bids." });

            // Select this technician
            interest.Status = InterestStatus.Selected;
            interest.ResponseDate = DateTime.UtcNow;

            // Assign technician to service request
            serviceRequest.ProfessionalId = interest.TechnicianId;
            serviceRequest.Status = ServiceRequestStatus.Matched;
            serviceRequest.EstimatedCost = interest.EstimatedCost;

            // Reject all other interests for this request
            var otherInterests = await _context.TechnicianInterests
                .Where(ti => ti.ServiceRequestId == serviceRequest.Id && ti.Id != interestId && ti.Status == InterestStatus.Interested)
                .ToListAsync();
            foreach (var other in otherInterests)
            {
                other.Status = InterestStatus.Rejected;
                other.ResponseDate = DateTime.UtcNow;
            }

            // Notify selected technician
            _context.Notifications.Add(new Notification
            {
                RecipientId = interest.TechnicianId,
                ServiceRequestId = serviceRequest.Id,
                Title = "U zgjodhët për punën!",
                Message = $"Ju u zgjodhët si teknik për kërkesën \"{serviceRequest.Title}\".",
                Type = NotificationType.TechnicianAssigned,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            // Notify citizen
            _context.Notifications.Add(new Notification
            {
                RecipientId = serviceRequest.CitizenId,
                ServiceRequestId = serviceRequest.Id,
                Title = "Tekniku u zgjodh",
                Message = $"Ju zgjodhët {interest.Technician?.FirstName} {interest.Technician?.LastName} për kërkesën tuaj.",
                Type = NotificationType.CaseMatched,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Push real-time notifications via SignalR
            await _notificationHub.Clients.Group($"user-{interest.TechnicianId}").SendAsync("ReceiveNotification", new
            {
                title = "U zgjodhët për punën!",
                message = $"Ju u zgjodhët si teknik për kërkesën \"{serviceRequest.Title}\".",
                serviceRequestId = serviceRequest.Id,
                type = (int)NotificationType.TechnicianAssigned
            });
            await _notificationHub.Clients.Group($"user-{serviceRequest.CitizenId}").SendAsync("ReceiveNotification", new
            {
                title = "Tekniku u zgjodh",
                message = $"Ju zgjodhët {interest.Technician?.FirstName} {interest.Technician?.LastName} për kërkesën tuaj.",
                serviceRequestId = serviceRequest.Id,
                type = (int)NotificationType.CaseMatched
            });

            return Ok(new { message = "Tekniku u zgjodh me sukses!", serviceRequest.Status, selectedTechnicianId = interest.TechnicianId });
        }

        /// <summary>
        /// Start work on a matched request - Professional changes status to InProgress.
        /// </summary>
        [HttpPost("{serviceRequestId}/start-work")]
        [Authorize(Roles = "Professional")]
        public async Task<IActionResult> StartWork(int serviceRequestId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var sr = await _context.ServiceRequests.FindAsync(serviceRequestId);
            if (sr == null) return NotFound();
            if (sr.ProfessionalId != userId) return Forbid();
            if (sr.Status != ServiceRequestStatus.Matched)
                return BadRequest(new { error = "Can only start work on matched requests." });

            sr.Status = ServiceRequestStatus.InProgress;
            sr.ScheduledDate = DateTime.UtcNow;

            _context.Notifications.Add(new Notification
            {
                RecipientId = sr.CitizenId,
                ServiceRequestId = sr.Id,
                Title = "Puna ka filluar!",
                Message = "Tekniku ka filluar punën për kërkesën tuaj.",
                Type = NotificationType.CaseInProgress,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Push real-time notification via SignalR
            await _notificationHub.Clients.Group($"user-{sr.CitizenId}").SendAsync("ReceiveNotification", new
            {
                title = "Puna ka filluar!",
                message = "Tekniku ka filluar punën për kërkesën tuaj.",
                serviceRequestId = sr.Id,
                type = (int)NotificationType.CaseInProgress
            });

            return Ok(new { message = "Work started!", sr.Status });
        }
    }

    public class ExpressInterestRequest
    {
        public string? PreventiveOffer { get; set; }
        public decimal? EstimatedCost { get; set; }
        public int? EstimatedTimeInHours { get; set; }
    }
}
