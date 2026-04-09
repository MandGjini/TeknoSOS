using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages
{
    public class TechnicianProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TechnicianProfileModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ApplicationUser Technician { get; set; } = null!;
        public List<ProfessionalSpecialty> Specialties { get; set; } = new();
        public List<TechnicianCertificate> Certificates { get; set; } = new();
        public List<TechnicianPortfolio> PortfolioItems { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public ProfileStats Stats { get; set; } = new();
        public PricingInfo Pricing { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class ProfileStats
        {
            public int TotalJobs { get; set; }
            public int CompletedJobs { get; set; }
            public int ActiveJobs { get; set; }
            public double ResponseRate { get; set; }
        }

        public class PricingInfo
        {
            public decimal? MinHourlyRate { get; set; }
            public decimal? MaxHourlyRate { get; set; }
            public decimal? AvgHourlyRate { get; set; }
            public int SpecialtiesWithPricing { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Try to find by ID first, then by DisplayUsername
            var tech = await _context.Users
                .Include(u => u.Specialties)
                .Include(u => u.Certificates)
                .Include(u => u.PortfolioItems)
                .Include(u => u.ReceivedReviews)
                    .ThenInclude(r => r.Reviewer)
                .FirstOrDefaultAsync(u => u.Id == id || u.DisplayUsername == id);

            if (tech == null) return NotFound();

            // Check if user is a professional
            var isInRole = await _userManager.IsInRoleAsync(tech, "Professional");
            if (!isInRole) return NotFound();

            Technician = tech;
            Specialties = Technician.Specialties.ToList();
            Certificates = Technician.Certificates.OrderByDescending(c => c.IssueDate).ToList();
            PortfolioItems = Technician.PortfolioItems.OrderByDescending(p => p.CompletedDate).ToList();
            Reviews = Technician.ReceivedReviews.OrderByDescending(r => r.CreatedDate).ToList(); // Show all reviews

            // Calculate pricing info
            var ratesWithValue = Specialties.Where(s => s.HourlyRate.HasValue && s.HourlyRate > 0).ToList();
            if (ratesWithValue.Any())
            {
                Pricing = new PricingInfo
                {
                    MinHourlyRate = ratesWithValue.Min(s => s.HourlyRate),
                    MaxHourlyRate = ratesWithValue.Max(s => s.HourlyRate),
                    AvgHourlyRate = Math.Round(ratesWithValue.Average(s => s.HourlyRate!.Value), 0),
                    SpecialtiesWithPricing = ratesWithValue.Count
                };
            }

            // Calculate stats
            var assignedRequests = await _context.ServiceRequests
                .Where(sr => sr.ProfessionalId == Technician.Id)
                .ToListAsync();

            Stats = new ProfileStats
            {
                TotalJobs = assignedRequests.Count,
                CompletedJobs = assignedRequests.Count(sr => sr.Status == Domain.Enums.ServiceRequestStatus.Completed),
                ActiveJobs = assignedRequests.Count(sr => sr.Status == Domain.Enums.ServiceRequestStatus.InProgress),
                ResponseRate = assignedRequests.Count > 0 ? Math.Round((double)assignedRequests.Count(sr => sr.Status != Domain.Enums.ServiceRequestStatus.Rejected) / assignedRequests.Count * 100, 1) : 0
            };

            return Page();
        }

        public async Task<IActionResult> OnPostStartChatAsync(string id)
        {
            // Ensure user is logged in
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrEmpty(id)) return BadRequest();

            // Find technician by id or display username
            var tech = await _context.Users.FirstOrDefaultAsync(u => u.Id == id || u.DisplayUsername == id);
            if (tech == null) return NotFound();

            // Prevent starting chat with self
            if (tech.Id == user.Id) return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id });

            // Try to find an existing open conversation (service request) between the two
            var existing = await _context.ServiceRequests
                .Where(sr => (sr.CitizenId == user.Id && sr.ProfessionalId == tech.Id) || (sr.CitizenId == tech.Id && sr.ProfessionalId == user.Id))
                .OrderByDescending(sr => sr.CreatedDate)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return RedirectToPage("/Chat", new { defectId = existing.Id });
            }

            // Create a minimal service request to host the chat
            var sr = new ServiceRequest
            {
                CitizenId = user.Id,
                ProfessionalId = tech.Id,
                UniqueCode = await GenerateUniqueCodeAsync(),
                Title = $"Bisedë me {tech.GetFullName()}",
                Description = "Bisedë iniciuar nga profili",
                Category = ServiceCategory.General,
                Status = ServiceRequestStatus.InProgress,
                Priority = ServiceRequestPriority.Normal,
                CasePriority = CasePriority.Minimal,
                CreatedDate = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Chat", new { defectId = sr.Id });
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            var lastRequest = await _context.ServiceRequests
                .OrderByDescending(sr => sr.Id)
                .Select(sr => sr.UniqueCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastRequest) && lastRequest.StartsWith("DEF-"))
            {
                if (int.TryParse(lastRequest.Substring(4), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"DEF-{nextNumber:D6}";
        }

        public IReadOnlyList<string> GetAdditionalImages(TechnicianPortfolio item)
        {
            if (string.IsNullOrWhiteSpace(item.AdditionalImagesJson))
            {
                return Array.Empty<string>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(item.AdditionalImagesJson) ?? new List<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public async Task<IActionResult> OnPostUpdatePortfolioAsync(
            string id,
            int portfolioId,
            string title,
            string? description,
            string? category,
            string? location,
            DateTime? completedDate,
            string? clientTestimonial,
            IFormFile? beforeImage,
            IFormFile? afterImage,
            List<IFormFile>? additionalImages,
            bool isFeatured = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (string.IsNullOrEmpty(id) || portfolioId <= 0) return BadRequest();

            var tech = await _context.Users.FirstOrDefaultAsync(u => u.Id == id || u.DisplayUsername == id);
            if (tech == null) return NotFound();

            if (tech.Id != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
                return Forbid();

            var portfolioItem = await _context.TechnicianPortfolios
                .FirstOrDefaultAsync(p => p.Id == portfolioId && p.TechnicianId == tech.Id);

            if (portfolioItem == null)
            {
                ErrorMessage = "Projekti i portfolio-s nuk u gjet.";
                return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id, fragment = "portfolio" });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ErrorMessage = "Titulli i projektit është i detyrueshëm.";
                return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id, fragment = "portfolio" });
            }

            try
            {
                if (beforeImage != null && beforeImage.Length > 0)
                {
                    portfolioItem.BeforeImageUrl = await SavePortfolioFileAsync(tech.Id, beforeImage);
                }

                if (afterImage != null && afterImage.Length > 0)
                {
                    portfolioItem.AfterImageUrl = await SavePortfolioFileAsync(tech.Id, afterImage);
                }

                if (additionalImages != null && additionalImages.Any(file => file != null && file.Length > 0))
                {
                    var additionalImageUrls = new List<string>();
                    foreach (var image in additionalImages.Where(file => file != null && file.Length > 0).Take(6))
                    {
                        var imageUrl = await SavePortfolioFileAsync(tech.Id, image);
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            additionalImageUrls.Add(imageUrl);
                        }
                    }

                    portfolioItem.AdditionalImagesJson = additionalImageUrls.Count > 0
                        ? JsonSerializer.Serialize(additionalImageUrls)
                        : null;
                }

                portfolioItem.Title = title.Trim();
                portfolioItem.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                portfolioItem.Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
                portfolioItem.Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
                portfolioItem.CompletedDate = completedDate;
                portfolioItem.ClientTestimonial = string.IsNullOrWhiteSpace(clientTestimonial) ? null : clientTestimonial.Trim();
                portfolioItem.IsFeatured = isFeatured;

                await _context.SaveChangesAsync();
                SuccessMessage = "Projekti i portfolio-s u përditësua me sukses.";
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id, fragment = "portfolio" });
        }

        public async Task<IActionResult> OnPostUploadPortfolioAsync(
            string id,
            string title,
            string? description,
            string? category,
            string? location,
            DateTime? completedDate,
            string? clientTestimonial,
            IFormFile? beforeImage,
            IFormFile? afterImage,
            List<IFormFile>? additionalImages,
            bool isFeatured = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Account/Login");

            if (string.IsNullOrEmpty(id)) return BadRequest();

            var tech = await _context.Users.Include(u => u.PortfolioItems).FirstOrDefaultAsync(u => u.Id == id || u.DisplayUsername == id);
            if (tech == null) return NotFound();

            // Only the profile owner or admins can upload
            if (tech.Id != user.Id && !await _userManager.IsInRoleAsync(user, "Admin"))
                return Forbid();

            if (string.IsNullOrWhiteSpace(title))
            {
                ErrorMessage = "Titulli i punës së përfunduar është i detyrueshëm.";
                return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id, fragment = "portfolio" });
            }

            if (beforeImage == null && afterImage == null && (additionalImages == null || additionalImages.Count == 0))
            {
                ErrorMessage = "Ngarko të paktën një foto për këtë punë të përfunduar.";
                return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id, fragment = "portfolio" });
            }

            try
            {
                var beforeImageUrl = await SavePortfolioFileAsync(tech.Id, beforeImage);
                var afterImageUrl = await SavePortfolioFileAsync(tech.Id, afterImage);

                var additionalImageUrls = new List<string>();
                if (additionalImages != null)
                {
                    foreach (var image in additionalImages.Where(file => file != null && file.Length > 0).Take(6))
                    {
                        var imageUrl = await SavePortfolioFileAsync(tech.Id, image);
                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            additionalImageUrls.Add(imageUrl);
                        }
                    }
                }

                var portfolioItem = new TechnicianPortfolio
                {
                    TechnicianId = tech.Id,
                    Title = title.Trim(),
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
                    Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
                    CompletedDate = completedDate,
                    ClientTestimonial = string.IsNullOrWhiteSpace(clientTestimonial) ? null : clientTestimonial.Trim(),
                    BeforeImageUrl = beforeImageUrl,
                    AfterImageUrl = afterImageUrl,
                    AdditionalImagesJson = additionalImageUrls.Count > 0 ? JsonSerializer.Serialize(additionalImageUrls) : null,
                    IsFeatured = isFeatured,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TechnicianPortfolios.Add(portfolioItem);
                await _context.SaveChangesAsync();
                SuccessMessage = "Puna e përfunduar u shtua me sukses në portfolio.";
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage(new { id = tech.DisplayUsername ?? tech.Id, fragment = "portfolio" });
        }

        private static readonly string[] AllowedPortfolioExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        private async Task<string?> SavePortfolioFileAsync(string technicianId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedPortfolioExtensions.Contains(ext))
            {
                throw new InvalidOperationException("Formati i fotos nuk lejohet në portfolio.");
            }

            if (file.Length > 8 * 1024 * 1024)
            {
                throw new InvalidOperationException("Secila foto e portfolio-s duhet të jetë më e vogël se 8MB.");
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "portfolio", technicianId);
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(uploadsDir, fileName);

            using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);

            return $"/uploads/portfolio/{technicianId}/{fileName}";
        }
    }
}
