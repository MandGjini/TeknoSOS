using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;

        public UsersModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public List<ApplicationUser> AllUsers { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public bool IncludeInactive { get; set; } = false;

        public int PageSize { get; set; } = 25;
        public int PageCount { get; set; }

        public int TotalCount { get; set; }
        public int AdminCount { get; set; }
        public int ProCount { get; set; }
        public int CitizenCount { get; set; }
        public int ActiveCount { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"]?.ToString();
            ErrorMessage = TempData["Error"]?.ToString();

            IQueryable<ApplicationUser> query = _db.Users.IgnoreQueryFilters();

            if (!IncludeInactive)
            {
                query = query.Where(u => u.IsActive ||
                    (u.Role == UserRole.Professional && u.SubscriptionEndDate.HasValue && u.SubscriptionEndDate.Value <= DateTime.UtcNow));
            }

            if (!string.IsNullOrEmpty(RoleFilter) && Enum.TryParse<UserRole>(RoleFilter, out var role))
                query = query.Where(u => u.Role == role);

            if (!string.IsNullOrEmpty(SearchQuery))
                query = query.Where(u =>
                    u.FirstName.Contains(SearchQuery) ||
                    u.LastName.Contains(SearchQuery) ||
                    (u.Email != null && u.Email.Contains(SearchQuery)) ||
                    (u.DisplayUsername != null && u.DisplayUsername.Contains(SearchQuery)));

            TotalCount = await query.CountAsync();
            PageCount = (int)Math.Ceiling((double)TotalCount / PageSize);

            AllUsers = await query.OrderByDescending(u => u.RegistrationDate)
                                  .Skip((Math.Max(1, PageNumber) - 1) * PageSize)
                                  .Take(PageSize)
                                  .ToListAsync();

            // Overall totals (non-filtered) for KPIs
            AdminCount = await _userManager.Users.CountAsync(u => u.Role == UserRole.Admin);
            ProCount = await _userManager.Users.CountAsync(u => u.Role == UserRole.Professional);
            CitizenCount = await _userManager.Users.CountAsync(u => u.Role == UserRole.Citizen);
            ActiveCount = await _userManager.Users.CountAsync(u => u.IsActive);
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                await TrySendUserChangeEmailAsync(
                    user,
                    user.IsActive ? "Llogaria juaj u aktivizua" : "Llogaria juaj u çaktivizua",
                    user.IsActive
                        ? "Statusi i llogarisë suaj në TeknoSOS u ndryshua në aktiv. Tani mund të vazhdoni përdorimin normal të platformës."
                        : "Statusi i llogarisë suaj në TeknoSOS u ndryshua në joaktive. Për sqarime të mëtejshme mund të kontaktoni administratorin e platformës.");
                TempData["Success"] = $"Statusi i {user.GetFullName()} u ndryshua.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(newRole)) return RedirectToPage();
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && Enum.TryParse<UserRole>(newRole, out var role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any()) await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
                user.Role = role;
                await _userManager.UpdateAsync(user);
                await TrySendUserChangeEmailAsync(
                    user,
                    "Roli juaj në TeknoSOS u përditësua",
                    $"Roli i llogarisë suaj u ndryshua në <strong>{newRole}</strong> nga administratori i platformës.");
                TempData["Success"] = $"Roli i {user.GetFullName()} u ndryshua ne {newRole}.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Protect main admin account and prevent self-deletion
                var currentUserId = _userManager.GetUserId(User);
                if (string.Equals(user.Email, "admin@teknosos.local", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Error"] = "Nuk mund te fshihet llogaria kryesore admin.";
                    return RedirectToPage();
                }
                if (user.Id == currentUserId)
                {
                    TempData["Error"] = "Nuk mund te fshini vetveten.";
                    return RedirectToPage();
                }

                var name = user.GetFullName();

                // Log audit entry
                try
                {
                    var audit = new TeknoSOS.WebApp.Domain.Entities.AuditLog
                    {
                        UserId = _userManager.GetUserId(User),
                        Action = "DeleteUser",
                        Entity = "ApplicationUser",
                        EntityId = null,
                        OldValue = System.Text.Json.JsonSerializer.Serialize(new { user.Id, user.Email, user.FirstName, user.LastName, user.Role }),
                        NewValue = null,
                        CreatedDate = DateTime.UtcNow,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString()
                    };
                    _db.AuditLogs.Add(audit);
                    await _db.SaveChangesAsync();
                }
                catch { }

                await _userManager.DeleteAsync(user);
                await TrySendUserChangeEmailAsync(
                    user,
                    "Llogaria juaj në TeknoSOS u fshi",
                    "Llogaria juaj u fshi nga administratori i platformës. Nëse mendoni se kjo është bërë gabimisht, ju lutem kontaktoni mbështetjen.");
                TempData["Success"] = $"Perdoruesi {name} u fshi.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeUsernameAsync(string userId, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                TempData["Error"] = "Username nuk mund te jete bosh.";
                return RedirectToPage();
            }
            newUsername = newUsername.Trim().ToLower().Replace(" ", "_");
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.DisplayUsername == newUsername && u.Id != userId);
            if (existing != null)
            {
                TempData["Error"] = $"Username '{newUsername}' ekziston.";
                return RedirectToPage();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.DisplayUsername = newUsername;
                await _userManager.UpdateAsync(user);
                await TrySendUserChangeEmailAsync(
                    user,
                    "Username juaj u ndryshua",
                    $"Username juaj në TeknoSOS u përditësua në <strong>@{newUsername}</strong> nga administratori i platformës.");
                TempData["Success"] = $"Username u ndryshua ne '@{newUsername}'.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateUserAsync(string firstName, string lastName, string email,
            string password, string role, string? city, string? phone, string? username)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email dhe password jane te detyrueshme.";
                return RedirectToPage();
            }
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = $"Email '{email}' ekziston.";
                return RedirectToPage();
            }
            if (!Enum.TryParse<UserRole>(role, out var userRole)) userRole = UserRole.Citizen;
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName ?? "",
                LastName = lastName ?? "",
                Role = userRole,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true,
                City = city,
                PhoneNumber = phone,
                DisplayUsername = !string.IsNullOrWhiteSpace(username) ? username.Trim().ToLower().Replace(" ", "_") : null,
                EmailConfirmed = true
            };
            try
            {
                var result = await _userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, role);
                    await TrySendUserChangeEmailAsync(
                        newUser,
                        "Llogaria juaj u krijua në TeknoSOS",
                        $"Administratori i platformës krijoi llogarinë tuaj me rolin <strong>{role}</strong>. Ju mund të hyni me email-in <strong>{email}</strong> dhe fjalëkalimin që ju është komunikuar.");
                    TempData["Success"] = $"Perdoruesi {firstName} {lastName} ({email}) u krijua me rolin {role}.";
                }
                else
                {
                    TempData["Error"] = "Gabim: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Gabim gjatë krijimit të përdoruesit: " + ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditUserAsync(string userId, string firstName, string lastName,
            string? city, string? phone, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.FirstName = firstName ?? user.FirstName;
                user.LastName = lastName ?? user.LastName;
                user.City = city;
                user.PhoneNumber = phone;
                user.IsActive = isActive;
                await _userManager.UpdateAsync(user);
                await TrySendUserChangeEmailAsync(
                    user,
                    "Të dhënat e llogarisë suaj u përditësuan",
                    "Administratori i platformës përditësoi disa të dhëna të profilit tuaj në TeknoSOS. Nëse nuk e prisnit këtë ndryshim, ju lutem kontaktoni mbështetjen.");
                TempData["Success"] = $"Te dhenat e {user.GetFullName()} u perditesuan.";
            }
            return RedirectToPage();
        }

        /// <summary>
        /// Batch update all technicians to have DisplayUsername in format "emer.mbiemer"
        /// </summary>
        public async Task<IActionResult> OnPostGenerateTechnicianUsernamesAsync()
        {
            var technicians = await _userManager.Users
                .Where(u => u.Role == UserRole.Professional)
                .ToListAsync();

            int updated = 0;
            var usedUsernames = new HashSet<string>();

            foreach (var tech in technicians)
            {
                if (!string.IsNullOrWhiteSpace(tech.FirstName) && !string.IsNullOrWhiteSpace(tech.LastName))
                {
                    var baseUsername = GenerateDisplayUsername(tech.FirstName, tech.LastName);
                    var finalUsername = baseUsername;
                    
                    // Handle duplicates by adding number suffix
                    int suffix = 1;
                    while (usedUsernames.Contains(finalUsername) || 
                           await _db.Users.AnyAsync(u => u.DisplayUsername == finalUsername && u.Id != tech.Id))
                    {
                        finalUsername = $"{baseUsername}{suffix}";
                        suffix++;
                    }
                    
                    usedUsernames.Add(finalUsername);
                    tech.DisplayUsername = finalUsername;
                    await _userManager.UpdateAsync(tech);
                    updated++;
                }
            }

            TempData["Success"] = $"U perditesuan {updated} teknikë me username emer.mbiemer";
            return RedirectToPage();
        }

        /// <summary>
        /// Seed certificates for all existing technicians who don't have any certificates yet.
        /// </summary>
        public async Task<IActionResult> OnPostSeedCertificatesAsync()
        {
            var technicians = await _userManager.Users
                .Include(u => u.Certificates)
                .Include(u => u.Specialties)
                .Where(u => u.Role == UserRole.Professional)
                .ToListAsync();

            int created = 0;

            foreach (var tech in technicians)
            {
                // Skip if already has certificates
                if (tech.Certificates.Any()) continue;

                // Get primary category from specialties
                var primarySpecialty = tech.Specialties.FirstOrDefault();
                var category = primarySpecialty?.Category ?? ServiceCategory.General;

                var certTemplates = GetCertificateTemplates(category);
                foreach (var cert in certTemplates)
                {
                    var issueDate = DateTime.UtcNow.AddYears(-Random.Shared.Next(1, 6)).AddMonths(-Random.Shared.Next(0, 12));
                    var expiryDate = cert.HasExpiry ? issueDate.AddYears(Random.Shared.Next(3, 6)) : (DateTime?)null;

                    _db.TechnicianCertificates.Add(new TechnicianCertificate
                    {
                        TechnicianId = tech.Id,
                        Title = cert.Title,
                        IssuedBy = cert.IssuedBy,
                        CertificateNumber = $"CERT-{Random.Shared.Next(10000, 99999)}-{DateTime.UtcNow.Year}",
                        IssueDate = issueDate,
                        ExpiryDate = expiryDate,
                        DocumentUrl = cert.DocumentUrl,
                        IsVerified = Random.Shared.Next(0, 3) > 0
                    });
                    created++;
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"U krijuan {created} certifikata për teknikët ekzistues";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendThankYouEmailAllAsync()
        {
            var users = await _userManager.Users
                .Where(u => u.Email != null && !u.Email.EndsWith("@teknosos.local"))
                .ToListAsync();
            int sent = 0;
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        await _emailSender.SendEmailAsync(
                            user.Email,
                            "Faleminderit që jeni pjesë e TeknoSOS.app",
                            "<p>Të falenderojmë që jeni pjesë e platformës TeknoSOS.app! <br>Ju urojmë sukses dhe eksperiencë të mirë në përdorim.<br><br>Stafi TeknoSOS.app</p>");
                        sent++;
                    }
                    catch { }
                }
            }
            TempData["Success"] = $"U dërguan {sent} email-e falenderimi.";
            return RedirectToPage();
        }

        private static List<(string Title, string IssuedBy, string? DocumentUrl, bool HasExpiry)> GetCertificateTemplates(ServiceCategory category)
        {
            var templates = new List<(string Title, string IssuedBy, string? DocumentUrl, bool HasExpiry)>();

            switch (category)
            {
                case ServiceCategory.Plumbing:
                    templates.Add(("Certifikatë Hidraulik Profesionist", "Ministria e Infrastrukturës", "/images/certs/plumbing-cert.jpg", true));
                    if (Random.Shared.Next(2) == 0)
                        templates.Add(("Licencë Punëtori Ujësjellës", "Ujësjellës Kanalizime Shqipëri", null, true));
                    break;

                case ServiceCategory.Electrical:
                    templates.Add(("Certifikatë Elektricist i Licencuar", "Enti Rregullator i Energjisë", "/images/certs/electrical-cert.jpg", true));
                    templates.Add(("Trajnim Siguria Elektrike", "Instituti i Sigurisë në Punë", null, true));
                    break;

                case ServiceCategory.HVAC:
                    templates.Add(("Certifikatë HVAC Specialist", "Shoqata e Klimatizimit Shqiptar", "/images/certs/hvac-cert.jpg", true));
                    if (Random.Shared.Next(2) == 0)
                        templates.Add(("Licencë Gazsjellës F-Gas", "Agjencia Kombëtare e Mjedisit", null, true));
                    break;

                case ServiceCategory.Carpentry:
                    templates.Add(("Certifikatë Marangoz Profesionist", "Dhoma e Zejtarisë Tiranë", "/images/certs/carpentry-cert.jpg", false));
                    break;

                case ServiceCategory.Appliance:
                    templates.Add(("Certifikatë Riparimi Elektroshtepiakesh", "Samsung Authorized Service", "/images/certs/appliance-cert.jpg", true));
                    if (Random.Shared.Next(2) == 0)
                        templates.Add(("Trajnim Teknik LG Electronics", "LG Albania", null, false));
                    break;

                case ServiceCategory.Mechanical:
                    templates.Add(("Certifikatë Mekanik Industrial", "Instituti Politeknik Tiranë", "/images/certs/mechanical-cert.jpg", false));
                    templates.Add(("Licencë Operimi Makinerish", "Inspektoriati Shtetëror Teknik", null, true));
                    break;

                case ServiceCategory.ITTechnology:
                    templates.Add(("CompTIA A+ Certification", "CompTIA International", "/images/certs/it-cert.jpg", true));
                    if (Random.Shared.Next(2) == 0)
                        templates.Add(("Cisco CCNA", "Cisco Systems", null, true));
                    if (Random.Shared.Next(3) == 0)
                        templates.Add(("Microsoft Certified: Azure Fundamentals", "Microsoft", null, false));
                    break;

                default: // General
                    templates.Add(("Certifikatë Mirëmbajtje e Përgjithshme", "Qendra e Formimit Profesional", "/images/certs/general-cert.jpg", false));
                    break;
            }

            return templates;
        }

        /// <summary>
        /// Generates a display username in format "emer.mbiemer" from first and last name.
        /// </summary>
        private static string GenerateDisplayUsername(string firstName, string lastName)
        {
            var normalizedFirst = NormalizeAlbanianText(firstName.Trim().ToLower());
            var normalizedLast = NormalizeAlbanianText(lastName.Trim().ToLower());
            
            normalizedFirst = System.Text.RegularExpressions.Regex.Replace(normalizedFirst, @"[^a-z0-9]", "");
            normalizedLast = System.Text.RegularExpressions.Regex.Replace(normalizedLast, @"[^a-z0-9]", "");
            
            return $"{normalizedFirst}.{normalizedLast}";
        }

        /// <summary>
        /// Normalizes Albanian special characters to their ASCII equivalents.
        /// </summary>
        private static string NormalizeAlbanianText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            return text
                .Replace("ë", "e")
                .Replace("ç", "c")
                .Replace("Ë", "E")
                .Replace("Ç", "C")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");
        }

        private async Task TrySendUserChangeEmailAsync(ApplicationUser user, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return;

            try
            {
                var fullName = string.IsNullOrWhiteSpace(user.GetFullName()) ? user.Email : user.GetFullName();
                var body = $@"<html><body style='font-family:Arial,sans-serif;line-height:1.6;'>
<p>Përshëndetje {fullName},</p>
<p>{message}</p>
<p>Për çdo pyetje, mund të kontaktoni administratorin ose mbështetjen e TeknoSOS.</p>
<p>Faleminderit,<br/>TeknoSOS</p>
</body></html>";

                await _emailSender.SendEmailAsync(user.Email, subject, body);
            }
            catch
            {
                // Email notification failures should not block admin operations.
            }
        }
    }
}
