using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BusinessesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public BusinessesModel(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public List<Business> Businesses { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public int? EditId { get; set; }
        [BindProperty] public string Name { get; set; } = string.Empty;
        [BindProperty] public string? Description { get; set; }
        [BindProperty] public string? Slogan { get; set; }
        [BindProperty] public BusinessType Type { get; set; } = BusinessType.Supplier;
        [BindProperty] public BusinessCategory PrimaryCategory { get; set; } = BusinessCategory.ConstructionMaterials;
        [BindProperty] public string? City { get; set; }
        [BindProperty] public string? Region { get; set; }
        [BindProperty] public string? ContactPhone { get; set; }
        [BindProperty] public string? ContactEmail { get; set; }
        [BindProperty] public string? PhysicalAddress { get; set; }
        [BindProperty] public string? LogoUrl { get; set; }
        [BindProperty] public bool IsActive { get; set; } = true;
        [BindProperty] public bool IsFeatured { get; set; }
        [BindProperty] public BusinessVerificationStatus VerificationStatus { get; set; } = BusinessVerificationStatus.Pending;

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"]?.ToString();
            ErrorMessage = TempData["Error"]?.ToString();

            Businesses = await _db.Businesses
                .OrderByDescending(b => b.IsFeatured)
                .ThenByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["Error"] = "Emri i biznesit eshte i detyrueshem.";
                return RedirectToPage();
            }

            var business = new Business
            {
                Name = Name.Trim(),
                Description = Description,
                Slogan = Slogan,
                Type = Type,
                PrimaryCategory = PrimaryCategory,
                City = City,
                Region = Region,
                ContactPhone = ContactPhone,
                ContactEmail = ContactEmail,
                PhysicalAddress = PhysicalAddress,
                LogoUrl = LogoUrl,
                IsActive = IsActive,
                IsFeatured = IsFeatured,
                VerificationStatus = VerificationStatus,
                VerificationDate = VerificationStatus == BusinessVerificationStatus.Verified ? DateTime.UtcNow : null,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _db.Businesses.Add(business);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Biznesi '{business.Name}' u krijua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!EditId.HasValue)
            {
                TempData["Error"] = "ID e biznesit mungon.";
                return RedirectToPage();
            }

            var business = await _db.Businesses.FindAsync(EditId.Value);
            if (business == null)
            {
                TempData["Error"] = "Biznesi nuk u gjet.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["Error"] = "Emri i biznesit eshte i detyrueshem.";
                return RedirectToPage();
            }

            business.Name = Name.Trim();
            business.Description = Description;
            business.Slogan = Slogan;
            business.Type = Type;
            business.PrimaryCategory = PrimaryCategory;
            business.City = City;
            business.Region = Region;
            business.ContactPhone = ContactPhone;
            business.ContactEmail = ContactEmail;
            business.PhysicalAddress = PhysicalAddress;
            business.LogoUrl = LogoUrl;
            business.IsActive = IsActive;
            business.IsFeatured = IsFeatured;
            business.VerificationStatus = VerificationStatus;
            business.VerificationDate = VerificationStatus == BusinessVerificationStatus.Verified
                ? (business.VerificationDate ?? DateTime.UtcNow)
                : null;
            business.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Biznesi '{business.Name}' u perditesua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var business = await _db.Businesses.FindAsync(id);
            if (business == null)
            {
                TempData["Error"] = "Biznesi nuk u gjet.";
                return RedirectToPage();
            }

            var businessName = business.Name;
            _db.Businesses.Remove(business);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Biznesi '{businessName}' u fshi me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var business = await _db.Businesses.FindAsync(id);
            if (business == null)
            {
                TempData["Error"] = "Biznesi nuk u gjet.";
                return RedirectToPage();
            }

            business.IsActive = !business.IsActive;
            business.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Biznesi '{business.Name}' u {(business.IsActive ? "aktivizua" : "caktivizua")}.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleFeaturedAsync(int id)
        {
            var business = await _db.Businesses.FindAsync(id);
            if (business == null)
            {
                TempData["Error"] = "Biznesi nuk u gjet.";
                return RedirectToPage();
            }

            business.IsFeatured = !business.IsFeatured;
            business.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Biznesi '{business.Name}' u {(business.IsFeatured ? "vendos" : "hoq")} nga lista featured.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUploadLogoAsync(IFormFile? businessLogo)
        {
            if (businessLogo == null || businessLogo.Length == 0)
                return new JsonResult(new { success = false, message = "Asnje skedar nuk u ngarkua." });

            if (businessLogo.Length > 5 * 1024 * 1024)
                return new JsonResult(new { success = false, message = "Skedari eshte shume i madh (max 5MB)." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            var ext = Path.GetExtension(businessLogo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return new JsonResult(new { success = false, message = "Lloji i skedarit nuk lejohet." });

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "businesses");
            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await businessLogo.CopyToAsync(stream);
            }

            return new JsonResult(new { success = true, url = $"/uploads/businesses/{fileName}" });
        }
    }
}
