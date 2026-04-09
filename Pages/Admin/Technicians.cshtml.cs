using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TechniciansModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly INotificationTemplateService _templateService;

        public TechniciansModel(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            IEmailSender emailSender,
            INotificationTemplateService templateService)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _templateService = templateService;
        }

        public List<TechnicianViewModel> Technicians { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public class TechnicianViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? City { get; set; }
            public string? CompanyName { get; set; }
            public decimal? AverageRating { get; set; }
            public int TotalReviews { get; set; }
            public int CompletedJobs { get; set; }
            public bool IsActive { get; set; }
            public bool IsProfileVerified { get; set; }
            public bool IsAvailable { get; set; }
            public int CertificateCount { get; set; }
            public int PortfolioCount { get; set; }
            public List<string> Specialties { get; set; } = new();
            public DateTime RegistrationDate { get; set; }
        }

        public async Task OnGetAsync()
        {
            var proUsers = await _userManager.GetUsersInRoleAsync("Professional");
            var proIds = proUsers.Select(u => u.Id).ToList();

            var techData = await _context.Users
                .Where(u => proIds.Contains(u.Id))
                .Select(u => new TechnicianViewModel
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Email = u.Email ?? "",
                    City = u.City,
                    CompanyName = u.CompanyName,
                    AverageRating = u.AverageRating,
                    TotalReviews = u.TotalReviews,
                    CompletedJobs = u.CompletedJobsCount,
                    IsActive = u.IsActive,
                    IsProfileVerified = u.IsProfileVerified,
                    IsAvailable = u.IsAvailableForWork,
                    CertificateCount = u.Certificates.Count,
                    PortfolioCount = u.PortfolioItems.Count,
                    Specialties = u.Specialties.Select(s => s.Category.ToString()).ToList(),
                    RegistrationDate = u.RegistrationDate
                })
                .ToListAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                techData = techData.Where(t => 
                    t.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.Email.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.City?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            if (StatusFilter == "verified") techData = techData.Where(t => t.IsProfileVerified).ToList();
            else if (StatusFilter == "unverified") techData = techData.Where(t => !t.IsProfileVerified).ToList();
            else if (StatusFilter == "inactive") techData = techData.Where(t => !t.IsActive).ToList();

            Technicians = techData.OrderByDescending(t => t.AverageRating ?? 0).ToList();
        }

        public async Task<IActionResult> OnPostVerifyAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsProfileVerified = true;
                await _userManager.UpdateAsync(user);

                // Send verification email to technician
                await SendTechnicianVerificationEmail(user);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                // Send activation/deactivation email to technician
                if (user.IsActive)
                {
                    await SendTechnicianActivationEmail(user);
                }
                else
                {
                    await SendTechnicianDeactivationEmail(user);
                }
            }
            return RedirectToPage();
        }

        private async Task SendTechnicianVerificationEmail(ApplicationUser user)
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

                var (subject, body) = await _templateService.RenderEmailTemplateAsync("technician_verified", placeholders);
                
                if (!string.IsNullOrEmpty(body))
                {
                    await _emailSender.SendEmailAsync(email, subject, body);
                }
            }
            catch (Exception)
            {
                // Don't fail the verification if email fails
            }
        }

        private async Task SendTechnicianActivationEmail(ApplicationUser user)
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

                var (subject, body) = await _templateService.RenderEmailTemplateAsync("technician_activated", placeholders);
                
                if (!string.IsNullOrEmpty(body))
                {
                    await _emailSender.SendEmailAsync(email, subject, body);
                }
            }
            catch (Exception)
            {
                // Don't fail the activation if email fails
            }
        }

        private async Task SendTechnicianDeactivationEmail(ApplicationUser user)
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

                var (subject, body) = await _templateService.RenderEmailTemplateAsync("technician_deactivated", placeholders);
                
                if (!string.IsNullOrEmpty(body))
                {
                    await _emailSender.SendEmailAsync(email, subject, body);
                }
            }
            catch (Exception)
            {
                // Don't fail the deactivation if email fails
            }
        }
    }
}
