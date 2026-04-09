using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class MenuManagerModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public MenuManagerModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty(SupportsGet = true)]
        public string Location { get; set; } = "main";

        public List<SiteMenu> MenuItems { get; set; } = new();
        public string? SuccessMsg { get; set; }

        // Edit form
        [BindProperty] public int EditId { get; set; }
        [BindProperty] public string EditMenuLocation { get; set; } = "main";
        [BindProperty] public string? EditTitle { get; set; }
        [BindProperty] public string? EditUrl { get; set; }
        [BindProperty] public string? EditIconClass { get; set; }
        [BindProperty] public string? EditCssClass { get; set; }
        [BindProperty] public bool EditOpenInNewTab { get; set; }
        [BindProperty] public string? EditRequiredRole { get; set; }
        [BindProperty] public bool EditRequiresAuth { get; set; }
        [BindProperty] public bool EditUnauthenticatedOnly { get; set; }
        [BindProperty] public int? EditParentId { get; set; }
        [BindProperty] public int EditSortOrder { get; set; }
        [BindProperty] public bool EditIsActive { get; set; } = true;

        public async Task OnGetAsync()
        {
            SuccessMsg = TempData["Success"]?.ToString();
            MenuItems = await _db.SiteMenus
                .Where(m => m.MenuLocation == Location)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Title)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (EditId > 0)
            {
                var existing = await _db.SiteMenus.FindAsync(EditId);
                if (existing != null)
                {
                    existing.MenuLocation = EditMenuLocation;
                    existing.Title = EditTitle ?? "";
                    existing.Url = EditUrl ?? "";
                    existing.IconClass = EditIconClass;
                    existing.CssClass = EditCssClass;
                    existing.OpenInNewTab = EditOpenInNewTab;
                    existing.RequiredRole = EditRequiredRole;
                    existing.RequiresAuth = EditRequiresAuth;
                    existing.UnauthenticatedOnly = EditUnauthenticatedOnly;
                    existing.ParentId = EditParentId;
                    existing.SortOrder = EditSortOrder;
                    existing.IsActive = EditIsActive;
                }
            }
            else
            {
                _db.SiteMenus.Add(new SiteMenu
                {
                    MenuLocation = EditMenuLocation,
                    Title = EditTitle ?? "",
                    Url = EditUrl ?? "",
                    IconClass = EditIconClass,
                    CssClass = EditCssClass,
                    OpenInNewTab = EditOpenInNewTab,
                    RequiredRole = EditRequiredRole,
                    RequiresAuth = EditRequiresAuth,
                    UnauthenticatedOnly = EditUnauthenticatedOnly,
                    ParentId = EditParentId,
                    SortOrder = EditSortOrder,
                    IsActive = EditIsActive
                });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = EditId > 0 ? "Menu u perditesua." : "Menu u shtua me sukses.";
            return RedirectToPage(new { location = EditMenuLocation });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _db.SiteMenus.FindAsync(id);
            if (item != null)
            {
                _db.SiteMenus.Remove(item);
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "Menu u fshi.";
            return RedirectToPage(new { location = Location });
        }
    }
}
