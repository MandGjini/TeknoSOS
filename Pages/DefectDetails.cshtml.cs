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
    public class DefectDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DefectDetailsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public ServiceRequest? Defect { get; set; }
        public List<Message> Messages { get; set; } = new();
        public List<TechnicianInterest> TechnicianInterests { get; set; } = new();
        public string CurrentUserId { get; set; } = "";
        public bool CanCancel { get; set; }
        public bool CanComplete { get; set; }
        public bool IsProfessional { get; set; }
        public bool IsOwner { get; set; }
        public bool HasExpressedInterest { get; set; }
        public bool CanBid { get; set; }
        public bool CanReview { get; set; }
        public Review? ExistingReview { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");
            CurrentUserId = user.Id;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            IsProfessional = await _userManager.IsInRoleAsync(user, "Professional");

            Defect = await _db.ServiceRequests
                .Include(s => s.Citizen)
                .Include(s => s.Professional)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (Defect == null) return Page();

            // Authorization - professionals can view any 'Created' defect to bid
            IsOwner = Defect.CitizenId == user.Id;
            if (!isAdmin && !IsOwner && Defect.ProfessionalId != user.Id && !(IsProfessional && Defect.Status == ServiceRequestStatus.Created))
            {
                return RedirectToPage("/DefectList");
            }

            Messages = await _db.Messages
                .Include(m => m.Sender)
                .Where(m => m.ServiceRequestId == id)
                .OrderBy(m => m.CreatedDate)
                .ToListAsync();

            TechnicianInterests = await _db.TechnicianInterests
                .Include(t => t.Technician)
                .Where(t => t.ServiceRequestId == id)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            // Bidding permissions
            HasExpressedInterest = IsProfessional &&
                await _db.TechnicianInterests.AnyAsync(ti =>
                    ti.TechnicianId == user.Id && ti.ServiceRequestId == id &&
                    ti.Status == InterestStatus.Interested);
            CanBid = IsProfessional && Defect.Status == ServiceRequestStatus.Created && !HasExpressedInterest;

            CanCancel = (Defect.CitizenId == user.Id || isAdmin) &&
                        Defect.Status != ServiceRequestStatus.Completed &&
                        Defect.Status != ServiceRequestStatus.Cancelled;

            CanComplete = (Defect.ProfessionalId == user.Id || isAdmin) &&
                          Defect.Status == ServiceRequestStatus.InProgress;

            // Review: citizen can review after completion, if no review exists yet
            ExistingReview = await _db.Reviews
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.ServiceRequestId == id);
            CanReview = IsOwner && Defect.Status == ServiceRequestStatus.Completed 
                        && Defect.ProfessionalId != null && ExistingReview == null;

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var defect = await _db.ServiceRequests.FindAsync(id);

            if (defect != null && (defect.CitizenId == user.Id || isAdmin))
            {
                defect.Status = ServiceRequestStatus.Cancelled;
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/DefectDetails", new { id });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var defect = await _db.ServiceRequests.FindAsync(id);

            if (defect != null && (defect.ProfessionalId == user.Id || isAdmin))
            {
                defect.Status = ServiceRequestStatus.Completed;
                defect.CompletedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return RedirectToPage("/DefectDetails", new { id });
        }

        public async Task<IActionResult> OnPostReviewAsync(int id, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            var defect = await _db.ServiceRequests.FindAsync(id);
            if (defect == null || defect.CitizenId != user.Id || defect.Status != ServiceRequestStatus.Completed || defect.ProfessionalId == null)
                return RedirectToPage("/DefectDetails", new { id });

            // Check if review already exists
            var existingReview = await _db.Reviews.AnyAsync(r => r.ServiceRequestId == id);
            if (existingReview)
                return RedirectToPage("/DefectDetails", new { id });

            // Validate rating
            if (rating < 1 || rating > 5) rating = 5;

            var review = new Review
            {
                ServiceRequestId = id,
                ReviewerId = user.Id,
                RevieweeId = defect.ProfessionalId,
                Rating = rating,
                Comment = comment ?? "",
                CreatedDate = DateTime.UtcNow
            };

            _db.Reviews.Add(review);

            // Update professional's average rating
            var professional = await _db.Users.FindAsync(defect.ProfessionalId);
            if (professional != null)
            {
                var allRatings = await _db.Reviews
                    .Where(r => r.RevieweeId == defect.ProfessionalId)
                    .Select(r => r.Rating)
                    .ToListAsync();
                allRatings.Add(rating);
                professional.AverageRating = (decimal)allRatings.Average();
                professional.TotalReviews = allRatings.Count;
            }

            // Notify the professional
            _db.Notifications.Add(new Notification
            {
                RecipientId = defect.ProfessionalId,
                ServiceRequestId = id,
                Title = "Vlerësim i ri!",
                Message = $"{user.FirstName} {user.LastName} ju vlerësoi me {rating} yje.",
                Type = NotificationType.ReviewReceived,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return RedirectToPage("/DefectDetails", new { id });
        }
    }
}
