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
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int ResolvedReports { get; set; }
        public int InProgressReports { get; set; }
        public int TotalMessages { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsGivenCount { get; set; }
        public int ReviewsReceivedCount { get; set; }
        public int LifetimeJobsCompleted { get; set; }
        public string UserRole { get; set; } = "Citizen";
        public string UserFullName { get; set; } = string.Empty;
        public int DaysLeft { get; set; }
        public bool IsSubscriptionExpired { get; set; }
        public bool IsSubscriptionExpiringSoon { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            UserFullName = user.GetFullName();
            var roles = await _userManager.GetRolesAsync(user);
            var userRoleString = roles.FirstOrDefault() ?? "Citizen";
            UserRole = userRoleString;

            var isAdmin = roles.Contains("Admin");
            IQueryable<ServiceRequest> requestsQuery;
            if (isAdmin)
            {
                requestsQuery = _context.ServiceRequests.AsQueryable();
            }
            else if (userRoleString == "Professional")
            {
                // Get technician's specialties
                var specialtyCategories = await _context.ProfessionalSpecialties
                    .Where(s => s.ProfessionalId == userId)
                    .Select(s => s.Category)
                    .ToListAsync();
                // Show only requests matching technician's specialties
                requestsQuery = _context.ServiceRequests
                    .Where(sr => specialtyCategories.Contains(sr.Category));
            }
            else
            {
                requestsQuery = _context.ServiceRequests.Where(sr => sr.CitizenId == userId || sr.ProfessionalId == userId);
            }

            TotalReports = await requestsQuery.CountAsync();
            PendingReports = await requestsQuery.CountAsync(sr => sr.Status == ServiceRequestStatus.Created);
            ResolvedReports = await requestsQuery.CountAsync(sr => sr.Status == ServiceRequestStatus.Completed);
            InProgressReports = await requestsQuery.CountAsync(sr => sr.Status == ServiceRequestStatus.InProgress);
            AverageRating = (double)(user.AverageRating ?? 0m);
            ReviewsGivenCount = await _context.Reviews.CountAsync(r => r.ReviewerId == userId);
            ReviewsReceivedCount = await _context.Reviews.CountAsync(r => r.RevieweeId == userId);
            LifetimeJobsCompleted = userRoleString == "Professional"
                ? await _context.ServiceRequests.CountAsync(sr => sr.ProfessionalId == userId && sr.Status == ServiceRequestStatus.Completed)
                : await _context.ServiceRequests.CountAsync(sr => sr.CitizenId == userId && sr.Status == ServiceRequestStatus.Completed);
            TotalMessages = await _context.Messages.CountAsync(m => m.SenderId == userId || m.ReceiverId == userId);

            if (userRoleString == "Professional")
            {
                var subscriptionEnd = user.SubscriptionEndDate ?? user.RegistrationDate.AddDays(90);
                DaysLeft = Math.Max(0, (int)Math.Ceiling((subscriptionEnd - DateTime.UtcNow).TotalDays));
                IsSubscriptionExpired = subscriptionEnd <= DateTime.UtcNow;
                IsSubscriptionExpiringSoon = !IsSubscriptionExpired && DaysLeft <= 5;
            }
            else
            {
                DaysLeft = 0;
                IsSubscriptionExpired = false;
                IsSubscriptionExpiringSoon = false;
            }

            ViewData["DaysLeft"] = DaysLeft;
            ViewData["IsSubscriptionExpired"] = IsSubscriptionExpired;
            ViewData["IsSubscriptionExpiringSoon"] = IsSubscriptionExpiringSoon;
        }

        public async Task<IActionResult> OnPostUploadDocumentAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Page();

            var file = Request.Form.Files["Document"];
            if (file != null && file.Length > 0)
            {
                // Save file to wwwroot/uploads/userId/
                var uploadsFolder = Path.Combine("wwwroot", "uploads", userId);
                Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Save document info to DB (pending confirmation)
                // You can create a DocumentUpload entity if needed
                // Example: _context.TechnicianCertificates.Add(new TechnicianCertificate { TechnicianId = userId, DocumentUrl = $"/uploads/{userId}/{file.FileName}", IsVerified = false, Title = file.FileName });
                await _context.SaveChangesAsync();
            }
            TempData["UploadStatus"] = "Dokumenti u ngarkua, do konfirmohet nga admin.";
            return RedirectToPage();
        }
    }
}
