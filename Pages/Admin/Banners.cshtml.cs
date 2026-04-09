using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BannersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public BannersModel(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public List<Banner> Banners { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        // Form binding properties
        [BindProperty]
        public int? EditId { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Position { get; set; } = "header";

        [BindProperty]
        public string? ImageUrl { get; set; }

        [BindProperty]
        public string? MobileImageUrl { get; set; }

        [BindProperty]
        public string? HtmlContent { get; set; }

        [BindProperty]
        public string? LinkUrl { get; set; }

        [BindProperty]
        public string LinkTarget { get; set; } = "_blank";

        [BindProperty]
        public string? AltText { get; set; }

        [BindProperty]
        public string TargetPages { get; set; } = "*";

        [BindProperty]
        public int DisplayOrder { get; set; }

        [BindProperty]
        public bool IsActive { get; set; } = true;

        [BindProperty]
        public DateTime? StartDate { get; set; }

        [BindProperty]
        public DateTime? EndDate { get; set; }

        [BindProperty]
        public string? BackgroundColor { get; set; }

        [BindProperty]
        public bool ShowCloseButton { get; set; }

        public async Task OnGetAsync()
        {
            Banners = await _db.Banners
                .OrderBy(b => b.Position)
                .ThenBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Emri i banerit është i detyrueshëm.";
                await OnGetAsync();
                return Page();
            }

            var banner = new Banner
            {
                Name = Name,
                Position = Position,
                ImageUrl = ImageUrl,
                MobileImageUrl = MobileImageUrl,
                HtmlContent = HtmlContent,
                LinkUrl = LinkUrl,
                LinkTarget = LinkTarget,
                AltText = AltText,
                TargetPages = TargetPages,
                DisplayOrder = DisplayOrder,
                IsActive = IsActive,
                StartDate = StartDate,
                EndDate = EndDate,
                BackgroundColor = BackgroundColor,
                ShowCloseButton = ShowCloseButton,
                CreatedDate = DateTime.UtcNow
            };

            _db.Banners.Add(banner);
            await _db.SaveChangesAsync();

            SuccessMessage = $"Baneri '{Name}' u krijua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!EditId.HasValue)
            {
                ErrorMessage = "ID e banerit mungon.";
                return RedirectToPage();
            }

            var banner = await _db.Banners.FindAsync(EditId.Value);
            if (banner == null)
            {
                ErrorMessage = "Baneri nuk u gjet.";
                return RedirectToPage();
            }

            banner.Name = Name;
            banner.Position = Position;
            banner.ImageUrl = ImageUrl;
            banner.MobileImageUrl = MobileImageUrl;
            banner.HtmlContent = HtmlContent;
            banner.LinkUrl = LinkUrl;
            banner.LinkTarget = LinkTarget;
            banner.AltText = AltText;
            banner.TargetPages = TargetPages;
            banner.DisplayOrder = DisplayOrder;
            banner.IsActive = IsActive;
            banner.StartDate = StartDate;
            banner.EndDate = EndDate;
            banner.BackgroundColor = BackgroundColor;
            banner.ShowCloseButton = ShowCloseButton;
            banner.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            SuccessMessage = $"Baneri '{Name}' u përditësua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner == null)
            {
                ErrorMessage = "Baneri nuk u gjet.";
                return RedirectToPage();
            }

            var name = banner.Name;
            _db.Banners.Remove(banner);
            await _db.SaveChangesAsync();

            SuccessMessage = $"Baneri '{name}' u fshi me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner == null)
            {
                ErrorMessage = "Baneri nuk u gjet.";
                return RedirectToPage();
            }

            banner.IsActive = !banner.IsActive;
            banner.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            SuccessMessage = $"Baneri '{banner.Name}' u {(banner.IsActive ? "aktivizua" : "çaktivizua")}.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUploadImageAsync(IFormFile? bannerImage)
        {
            if (bannerImage == null || bannerImage.Length == 0)
            {
                return new JsonResult(new { success = false, message = "Asnjë skedar nuk u ngarkua." });
            }

            if (bannerImage.Length > 5 * 1024 * 1024) // 5MB max
            {
                return new JsonResult(new { success = false, message = "Skedari është shumë i madh (max 5MB)." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            var ext = Path.GetExtension(bannerImage.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                return new JsonResult(new { success = false, message = "Lloji i skedarit nuk lejohet." });
            }

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "banners");
            Directory.CreateDirectory(uploadPath);

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bannerImage.CopyToAsync(stream);
            }

            var url = $"/uploads/banners/{uniqueName}";
            return new JsonResult(new { success = true, url });
        }

        public async Task<IActionResult> OnPostResetStatsAsync(int id)
        {
            var banner = await _db.Banners.FindAsync(id);
            if (banner == null)
            {
                ErrorMessage = "Baneri nuk u gjet.";
                return RedirectToPage();
            }

            banner.ClickCount = 0;
            banner.ViewCount = 0;
            banner.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            SuccessMessage = $"Statistikat e banerit '{banner.Name}' u rivendosën.";
            return RedirectToPage();
        }
    }
}
