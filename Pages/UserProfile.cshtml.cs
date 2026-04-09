using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    [Authorize]
    public class UserProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser ViewedUser { get; set; } = null!;
        public bool IsOwnProfile { get; set; }
        public int CreatedRequestsCount { get; set; }
        public int CompletedRequestsCount { get; set; }
        public int WrittenReviewsCount { get; set; }
        public List<Review> RecentReviewsWritten { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id || u.DisplayUsername == id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.Role == UserRole.Professional)
            {
                return RedirectToPage("/TechnicianProfile", new { id = user.DisplayUsername ?? user.Id });
            }

            ViewedUser = user;

            var currentUser = await _userManager.GetUserAsync(User);
            IsOwnProfile = currentUser?.Id == ViewedUser.Id;

            CreatedRequestsCount = await _context.ServiceRequests
                .CountAsync(sr => sr.CitizenId == ViewedUser.Id);

            CompletedRequestsCount = await _context.ServiceRequests
                .CountAsync(sr => sr.CitizenId == ViewedUser.Id && sr.Status == ServiceRequestStatus.Completed);

            WrittenReviewsCount = await _context.Reviews
                .CountAsync(r => r.ReviewerId == ViewedUser.Id);

            RecentReviewsWritten = await _context.Reviews
                .Where(r => r.ReviewerId == ViewedUser.Id)
                .Include(r => r.Reviewee)
                .OrderByDescending(r => r.CreatedDate)
                .Take(5)
                .ToListAsync();

            return Page();
        }
    }
}