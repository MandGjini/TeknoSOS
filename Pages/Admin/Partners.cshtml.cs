using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PartnersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PartnersModel(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public List<Partner> Partners { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public int? EditId { get; set; }
        [BindProperty] public string Name { get; set; } = string.Empty;
        [BindProperty] public string? LogoUrl { get; set; }
        [BindProperty] public string? WebsiteUrl { get; set; }
        [BindProperty] public string? Description { get; set; }
        [BindProperty] public int DisplayOrder { get; set; }
        [BindProperty] public bool IsActive { get; set; } = true;

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"]?.ToString();
            ErrorMessage = TempData["Error"]?.ToString();
            Partners = await _db.Partners.OrderBy(p => p.DisplayOrder).ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["Error"] = "Emri i partnerit eshte i detyrueshem.";
                return RedirectToPage();
            }

            _db.Partners.Add(new Partner
            {
                Name = Name,
                LogoUrl = LogoUrl,
                WebsiteUrl = WebsiteUrl,
                Description = Description,
                DisplayOrder = DisplayOrder,
                IsActive = IsActive,
                CreatedDate = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Partneri '{Name}' u krijua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!EditId.HasValue)
            {
                TempData["Error"] = "ID e partnerit mungon.";
                return RedirectToPage();
            }

            var partner = await _db.Partners.FindAsync(EditId.Value);
            if (partner == null)
            {
                TempData["Error"] = "Partneri nuk u gjet.";
                return RedirectToPage();
            }

            partner.Name = Name;
            partner.LogoUrl = LogoUrl;
            partner.WebsiteUrl = WebsiteUrl;
            partner.Description = Description;
            partner.DisplayOrder = DisplayOrder;
            partner.IsActive = IsActive;
            partner.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Partneri '{Name}' u perditesua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var partner = await _db.Partners.FindAsync(id);
            if (partner == null)
            {
                TempData["Error"] = "Partneri nuk u gjet.";
                return RedirectToPage();
            }

            var name = partner.Name;
            _db.Partners.Remove(partner);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Partneri '{name}' u fshi me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var partner = await _db.Partners.FindAsync(id);
            if (partner == null)
            {
                TempData["Error"] = "Partneri nuk u gjet.";
                return RedirectToPage();
            }

            partner.IsActive = !partner.IsActive;
            partner.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Partneri '{partner.Name}' u {(partner.IsActive ? "aktivizua" : "caktivizua")}.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUploadLogoAsync(IFormFile? partnerLogo)
        {
            if (partnerLogo == null || partnerLogo.Length == 0)
                return new JsonResult(new { success = false, message = "Asnje skedar nuk u ngarkua." });

            if (partnerLogo.Length > 5 * 1024 * 1024)
                return new JsonResult(new { success = false, message = "Skedari eshte shume i madh (max 5MB)." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            var ext = Path.GetExtension(partnerLogo.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
                return new JsonResult(new { success = false, message = "Lloji i skedarit nuk lejohet." });

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "partners");
            Directory.CreateDirectory(uploadPath);

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await partnerLogo.CopyToAsync(stream);
            }

            return new JsonResult(new { success = true, url = $"/uploads/partners/{uniqueName}" });
        }
    }
}
