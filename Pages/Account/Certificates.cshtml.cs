using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages.Account
{
    [Authorize]
    public class CertificatesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CertificatesModel> _logger;

        public CertificatesModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IWebHostEnvironment environment,
            ILogger<CertificatesModel> logger)
        {
            _userManager = userManager;
            _db = db;
            _environment = environment;
            _logger = logger;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public List<TechnicianCertificate> Certificates { get; set; } = new();
        public string? ProfileImageUrl { get; set; }
        public bool HasUploadedCertificates { get; set; }
        public bool HasUploadedProfilePhoto { get; set; }
        public bool IsProfileVerified { get; set; }
        public bool IsInTrialPeriod { get; set; }
        public int TrialDaysRemaining { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Only professionals can access this page
            if (CurrentUser.Role != UserRole.Professional)
            {
                return RedirectToPage("/Account/Profile");
            }

            await LoadUserDataAsync();
            return Page();
        }

        private async Task LoadUserDataAsync()
        {
            if (CurrentUser == null) return;

            Certificates = await _db.TechnicianCertificates
                .Where(c => c.TechnicianId == CurrentUser.Id)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ProfileImageUrl = CurrentUser.ProfileImageUrl;
            HasUploadedCertificates = CurrentUser.HasUploadedCertificates || Certificates.Any();
            HasUploadedProfilePhoto = CurrentUser.HasUploadedProfilePhoto || !string.IsNullOrEmpty(CurrentUser.ProfileImageUrl);
            IsProfileVerified = CurrentUser.IsProfileVerified;
            IsInTrialPeriod = CurrentUser.IsInTrialPeriod;
            TrialDaysRemaining = CurrentUser.TrialDaysRemaining;
        }

        public async Task<IActionResult> OnPostUploadProfilePhotoAsync(IFormFile profilePhoto)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null || CurrentUser.Role != UserRole.Professional)
            {
                return RedirectToPage("/Account/Login");
            }

            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                ErrorMessage = "Ju lutem zgjidhni një foto për të ngarkuar.";
                await LoadUserDataAsync();
                return Page();
            }

            // Validate file size (max 5MB)
            if (profilePhoto.Length > 5 * 1024 * 1024)
            {
                ErrorMessage = "Foto duhet të jetë më e vogël se 5MB.";
                await LoadUserDataAsync();
                return Page();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(profilePhoto.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ErrorMessage = "Formati i fotos nuk lejohet. Përdorni JPG, PNG ose WebP.";
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{CurrentUser.Id}_{DateTime.UtcNow.Ticks}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Delete old profile photo if exists
                if (!string.IsNullOrEmpty(CurrentUser.ProfileImageUrl))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, CurrentUser.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Save new photo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePhoto.CopyToAsync(stream);
                }

                // Update user
                CurrentUser.ProfileImageUrl = $"/uploads/profiles/{fileName}";
                CurrentUser.HasUploadedProfilePhoto = true;
                await _userManager.UpdateAsync(CurrentUser);

                SuccessMessage = "Foto e profilit u ngarkua me sukses!";
                _logger.LogInformation("Profile photo uploaded for user {UserId}", CurrentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload profile photo for user {UserId}", CurrentUser.Id);
                ErrorMessage = "Ndodhi një gabim gjatë ngarkimit të fotos. Ju lutem provoni përsëri.";
            }

            await LoadUserDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadCertificateAsync(
            string title,
            string? issuedBy,
            string? certificateNumber,
            DateTime? issueDate,
            DateTime? expiryDate,
            IFormFile certificateFile)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null || CurrentUser.Role != UserRole.Professional)
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ErrorMessage = "Titulli i certifikatës është i detyrueshëm.";
                await LoadUserDataAsync();
                return Page();
            }

            if (certificateFile == null || certificateFile.Length == 0)
            {
                ErrorMessage = "Ju lutem zgjidhni një dokument për të ngarkuar.";
                await LoadUserDataAsync();
                return Page();
            }

            // Validate file size (max 10MB)
            if (certificateFile.Length > 10 * 1024 * 1024)
            {
                ErrorMessage = "Dokumenti duhet të jetë më i vogël se 10MB.";
                await LoadUserDataAsync();
                return Page();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            var extension = Path.GetExtension(certificateFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ErrorMessage = "Formati i dokumentit nuk lejohet. Përdorni JPG, PNG, WebP ose PDF.";
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "certificates");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{CurrentUser.Id}_{DateTime.UtcNow.Ticks}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await certificateFile.CopyToAsync(stream);
                }

                // Create certificate record
                var certificate = new TechnicianCertificate
                {
                    TechnicianId = CurrentUser.Id,
                    Title = title.Trim(),
                    IssuedBy = issuedBy?.Trim(),
                    CertificateNumber = certificateNumber?.Trim(),
                    IssueDate = issueDate,
                    ExpiryDate = expiryDate,
                    DocumentUrl = $"/uploads/certificates/{fileName}",
                    IsVerified = false, // Needs admin verification
                    CreatedDate = DateTime.UtcNow
                };

                _db.TechnicianCertificates.Add(certificate);

                // Update user flag
                CurrentUser.HasUploadedCertificates = true;
                await _userManager.UpdateAsync(CurrentUser);
                
                await _db.SaveChangesAsync();

                SuccessMessage = "Certifikata u ngarkua me sukses! Stafi ynë do ta verifikojë së shpejti.";
                _logger.LogInformation("Certificate '{Title}' uploaded for user {UserId}", title, CurrentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload certificate for user {UserId}", CurrentUser.Id);
                ErrorMessage = "Ndodhi një gabim gjatë ngarkimit të certifikatës. Ju lutem provoni përsëri.";
            }

            await LoadUserDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCertificateAsync(int certificateId)
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null || CurrentUser.Role != UserRole.Professional)
            {
                return RedirectToPage("/Account/Login");
            }

            var certificate = await _db.TechnicianCertificates
                .FirstOrDefaultAsync(c => c.Id == certificateId && c.TechnicianId == CurrentUser.Id);

            if (certificate == null)
            {
                ErrorMessage = "Certifikata nuk u gjet.";
                await LoadUserDataAsync();
                return Page();
            }

            try
            {
                // Delete file from disk
                if (!string.IsNullOrEmpty(certificate.DocumentUrl))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, certificate.DocumentUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Remove from database
                _db.TechnicianCertificates.Remove(certificate);
                await _db.SaveChangesAsync();

                // Check if there are any remaining certificates
                var remainingCertificates = await _db.TechnicianCertificates
                    .AnyAsync(c => c.TechnicianId == CurrentUser.Id);

                if (!remainingCertificates)
                {
                    CurrentUser.HasUploadedCertificates = false;
                    await _userManager.UpdateAsync(CurrentUser);
                }

                SuccessMessage = "Certifikata u fshi me sukses.";
                _logger.LogInformation("Certificate {CertificateId} deleted for user {UserId}", certificateId, CurrentUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete certificate {CertificateId} for user {UserId}", certificateId, CurrentUser.Id);
                ErrorMessage = "Ndodhi një gabim gjatë fshirjes së certifikatës.";
            }

            await LoadUserDataAsync();
            return Page();
        }
    }
}
