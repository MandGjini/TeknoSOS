using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userRoles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException());

            var isAdmin = userRoles.Contains("Admin");

            // Base query - admins see all, others see their own
            var requestsQuery = isAdmin
                ? _context.ServiceRequests.AsQueryable()
                : _context.ServiceRequests.Where(sr => sr.CitizenId == userId || sr.ProfessionalId == userId);

            var totalRequests = await requestsQuery.CountAsync();
            var activeRequests = await requestsQuery.CountAsync(sr =>
                sr.Status == Domain.Enums.ServiceRequestStatus.InProgress ||
                sr.Status == Domain.Enums.ServiceRequestStatus.Matched);
            var completedRequests = await requestsQuery.CountAsync(sr =>
                sr.Status == Domain.Enums.ServiceRequestStatus.Completed);
            var pendingRequests = await requestsQuery.CountAsync(sr =>
                sr.Status == Domain.Enums.ServiceRequestStatus.Created);

            var unreadNotifications = await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead);

            var recentRequests = await requestsQuery
                .OrderByDescending(sr => sr.CreatedDate)
                .Take(5)
                .Select(sr => new
                {
                    sr.Id,
                    sr.Title,
                    sr.Status,
                    sr.Priority,
                    sr.Category,
                    sr.CreatedDate
                })
                .ToListAsync();

            return Ok(new
            {
                stats = new
                {
                    totalRequests,
                    activeRequests,
                    completedRequests,
                    pendingRequests,
                    unreadNotifications
                },
                recentRequests
            });
        }
    }
}
