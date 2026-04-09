using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages
{
    public class SetupModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ISiteSettingsService _settings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SetupModel(
            ApplicationDbContext db,
            ISiteSettingsService settings,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _settings = settings;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public bool IsSetupCompleted { get; set; }
        public bool DbConnected { get; set; }
        public bool TablesExist { get; set; }
        public int UserCount { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty] public string PlatformName { get; set; } = "TeknoSOS";
        [BindProperty] public string PlatformTagline { get; set; } = "Platforma Dixhitale e Sherbimeve Teknike";
        [BindProperty] public string AdminEmail { get; set; } = "admin@teknosos.local";
        [BindProperty] public string AdminPassword { get; set; } = "";
        [BindProperty] public string AdminFirstName { get; set; } = "System";
        [BindProperty] public string AdminLastName { get; set; } = "Administrator";
        [BindProperty] public string DefaultLanguage { get; set; } = "sq";
        [BindProperty] public string ContactEmail { get; set; } = "info@teknosos.al";
        [BindProperty] public string ContactPhone { get; set; } = "+355 69 123 4567";

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if setup already completed
            var setupDone = await _settings.GetAsync("general", "SetupCompleted", "false");
            IsSetupCompleted = setupDone == "true";

            // If already set up and not admin, redirect home
            if (IsSetupCompleted && !(User.Identity?.IsAuthenticated == true && User.IsInRole("Admin")))
            {
                return RedirectToPage("/Index");
            }

            await CheckSystemStatus();
            return Page();
        }

        private async Task CheckSystemStatus()
        {
            try
            {
                DbConnected = await _db.Database.CanConnectAsync();
                if (DbConnected)
                {
                    TablesExist = await _db.Users.AnyAsync() || true; // If query runs, tables exist
                    UserCount = await _db.Users.CountAsync();
                }
            }
            catch
            {
                DbConnected = false;
            }
        }

        public async Task<IActionResult> OnPostRunMigrationsAsync()
        {
            try
            {
                await _db.Database.MigrateAsync();
                SuccessMessage = "Migrimet u ekzekutuan me sukses. Databaza eshte gati.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Gabim gjate migrimit: {ex.Message}";
            }
            await CheckSystemStatus();
            return Page();
        }

        public async Task<IActionResult> OnPostCompleteSetupAsync()
        {
            try
            {
                // 1. Ensure migrations are applied
                await _db.Database.MigrateAsync();

                // 2. Ensure roles
                string[] roles = { "Admin", "Professional", "Citizen" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // 3. Create or update admin
                if (!string.IsNullOrEmpty(AdminPassword) && AdminPassword.Length >= 6)
                {
                    var existing = await _userManager.FindByEmailAsync(AdminEmail);
                    if (existing == null)
                    {
                        var adminUser = new ApplicationUser
                        {
                            UserName = AdminEmail,
                            Email = AdminEmail,
                            FirstName = AdminFirstName,
                            LastName = AdminLastName,
                            EmailConfirmed = true,
                            IsActive = true,
                            RegistrationDate = DateTime.UtcNow
                        };
                        try
                        {
                            var result = await _userManager.CreateAsync(adminUser, AdminPassword);
                            if (result.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(adminUser, "Admin");
                            }
                            else
                            {
                                ErrorMessage = "Gabim ne krijimin e admin: " + string.Join(", ", result.Errors.Select(e => e.Description));
                                await CheckSystemStatus();
                                return Page();
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = "Gabim gjatë krijimit të admin: " + ex.Message;
                            await CheckSystemStatus();
                            return Page();
                        }
                    }
                }

                // 4. Save platform settings
                await _settings.SetGroupAsync("branding", new Dictionary<string, string>
                {
                    ["SiteName"] = PlatformName,
                    ["SiteTagline"] = PlatformTagline
                });

                await _settings.SetGroupAsync("contact", new Dictionary<string, string>
                {
                    ["ContactEmail"] = ContactEmail,
                    ["ContactPhone"] = ContactPhone
                });

                await _settings.SetGroupAsync("languages", new Dictionary<string, string>
                {
                    ["DefaultLanguage"] = DefaultLanguage,
                    ["ActiveLanguages"] = "sq,en,it,de,fr"
                });

                // 5. Mark setup as completed
                await _settings.SetAsync("general", "SetupCompleted", "true");

                // 6. Seed CMS content if needed
                var contentService = HttpContext.RequestServices.GetRequiredService<ISiteContentService>();
                if (!await contentService.HasContentAsync("home", "sq"))
                {
                    var defaultContent = ContentSeeder.GetDefaultContent();
                    await contentService.SaveBulkContentAsync(defaultContent, "setup-wizard");
                }

                SuccessMessage = "Konfigurimi perfundoi me sukses! Platforma eshte gati per perdorim.";
                IsSetupCompleted = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Gabim: {ex.Message}";
            }

            await CheckSystemStatus();
            return Page();
        }
    }
}
