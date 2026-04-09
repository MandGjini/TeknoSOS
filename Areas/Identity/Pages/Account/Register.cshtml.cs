using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Hubs;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ApplicationDbContext _context;
        private readonly INotificationTemplateService _templateService;

        private readonly IConfiguration _configuration;
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IHubContext<NotificationHub> notificationHub,
            ApplicationDbContext context,
            INotificationTemplateService templateService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _notificationHub = notificationHub;
            _context = context;
            _templateService = templateService;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }

        public class InputModel
        {
            // Step 1: Account Type Selection
            [Required(ErrorMessage = "Ju lutem zgjidhni llojin e llogarisë")]
            [Display(Name = "Lloji i Llogarisë")]
            public UserRole AccountType { get; set; } = UserRole.Citizen;

            // Step 2: Basic Info
            [Required(ErrorMessage = "Emri është i detyrueshëm")]
            [StringLength(50, ErrorMessage = "Emri duhet të jetë maksimum {1} karaktere")]
            [Display(Name = "Emri")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mbiemri është i detyrueshëm")]
            [StringLength(50, ErrorMessage = "Mbiemri duhet të jetë maksimum {1} karaktere")]
            [Display(Name = "Mbiemri")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email-i është i detyrueshëm")]
            [EmailAddress(ErrorMessage = "Formati i email-it nuk është i vlefshëm")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Numri i telefonit është i detyrueshëm")]
            [Phone(ErrorMessage = "Formati i numrit të telefonit nuk është i vlefshëm")]
            [Display(Name = "Numri i Telefonit")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Fjalëkalimi është i detyrueshëm")]
            [StringLength(100, ErrorMessage = "Fjalëkalimi duhet të jetë të paktën {2} karaktere", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Fjalëkalimi")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Konfirmo Fjalëkalimin")]
            [Compare("Password", ErrorMessage = "Fjalëkalimet nuk përputhen")]
            public string ConfirmPassword { get; set; } = string.Empty;

            // Step 3: Location
            [Display(Name = "Qyteti")]
            public string? City { get; set; }

            [Display(Name = "Adresa")]
            public string? Address { get; set; }

            // Professional-specific fields (Step 4)
            [Display(Name = "Kategoria e Shërbimit")]
            public ServiceCategory? PrimaryCategory { get; set; }

            [Display(Name = "Kategori të Tjera")]
            public List<ServiceCategory>? AdditionalCategories { get; set; }

            [Display(Name = "Vitet e Eksperiencës")]
            [Range(0, 50, ErrorMessage = "Vitet e eksperiencës duhet të jenë midis 0 dhe 50")]
            public int? YearsOfExperience { get; set; }

            [Display(Name = "Emri i Kompanisë (nëse ka)")]
            [StringLength(100)]
            public string? CompanyName { get; set; }

            [Display(Name = "Numri i Licencës (nëse ka)")]
            [StringLength(50)]
            public string? LicenseNumber { get; set; }

            [Display(Name = "Përshkrimi/Bio")]
            [StringLength(500, ErrorMessage = "Bio duhet të jetë maksimum {1} karaktere")]
            public string? Bio { get; set; }

            [Display(Name = "Rrezja e Shërbimit (km)")]
            [Range(1, 100, ErrorMessage = "Rrezja duhet të jetë midis 1 dhe 100 km")]
            public int? ServiceRadiusKm { get; set; }

            // Terms acceptance
            [Range(typeof(bool), "true", "true", ErrorMessage = "Duhet të pranoni Kushtet e Shërbimit")]
            [Display(Name = "Pranoj Kushtet e Shërbimit dhe Politikën e Privatësisë")]
            public bool AcceptTerms { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Check config flag for technician self-registration
                bool techSelfRegEnabled = _configuration.GetValue<bool>("Registration:TechnicianSelfRegistrationEnabled", false);
                if (Input.AccountType == UserRole.Professional && !techSelfRegEnabled)
                {
                    ModelState.AddModelError(string.Empty, "Regjistrimi i teknikëve është i mbyllur për publikun. Vetëm administratori mund të krijojë llogari teknikësh.");
                    return Page();
                }

                var user = CreateUser();

                // Set basic properties
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.PhoneNumber = Input.PhoneNumber;
                user.Role = Input.AccountType;
                user.RegistrationDate = DateTime.UtcNow;
                user.City = Input.City;
                user.Address = Input.Address;

                // Activation logic
                if (Input.AccountType == UserRole.Citizen)
                {
                    user.IsActive = true;
                }
                else if (Input.AccountType == UserRole.Professional)
                {
                    user.IsActive = false;
                    user.YearsOfExperience = Input.YearsOfExperience;
                    user.CompanyName = Input.CompanyName;
                    user.LicenseNumber = Input.LicenseNumber;
                    user.Bio = Input.Bio;
                    user.ServiceRadiusKm = Input.ServiceRadiusKm;
                    user.IsAvailableForWork = false;
                    user.IsProfileVerified = false;
                    user.HasUploadedCertificates = false;
                    user.HasUploadedProfilePhoto = false;
                    user.DisplayUsername = GenerateDisplayUsername(Input.FirstName, Input.LastName);
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Prevent duplicate registrations
                var existing = await _userManager.FindByEmailAsync(Input.Email);
                if (existing != null)
                {
                    ModelState.AddModelError(string.Empty, "Email-i është regjistruar tashmë.");
                    return Page();
                }

                try
                {
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add to role
                    var roleName = Input.AccountType.ToString();
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Notify admins when a new technician registers
                    if (Input.AccountType == UserRole.Professional)
                    {
                        await NotifyAdminsNewTechnician(user);
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Send different emails for technicians vs citizens
                    if (Input.AccountType == UserRole.Professional)
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            { "FirstName", Input.FirstName ?? "Teknik" },
                            { "LastName", Input.LastName ?? "" },
                            { "Email", Input.Email },
                            { "ConfirmationUrl", HtmlEncoder.Default.Encode(callbackUrl!) }
                        };

                        var (subject, body) = await _templateService.RenderEmailTemplateAsync("technician_welcome_registration", placeholders);
                        if (!string.IsNullOrEmpty(body))
                        {
                            await _emailSender.SendEmailAsync(Input.Email, subject, body);
                        }
                        else
                        {
                            await _emailSender.SendEmailAsync(Input.Email, "Mirë se vini në TeknoSOS - Regjistrimi si Teknik",
                                $@"<html>
                                <body style='font-family: Arial, sans-serif;'>
                                    <h2>Mirë se vini, {Input.FirstName}!</h2>
                                    <p>Konfirmo email-in duke klikuar <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>këtu</a>.</p>
                                    <p>Llogaria juaj do verifikohet nga stafi brenda 24-48 orëve.</p>
                                </body>
                                </html>");
                        }
                    }
                    else
                    {
                        var placeholders = new Dictionary<string, string>
                        {
                            { "FirstName", Input.FirstName ?? "Përdorues" },
                            { "LastName", Input.LastName ?? "" },
                            { "Email", Input.Email },
                            { "ConfirmationUrl", HtmlEncoder.Default.Encode(callbackUrl!) }
                        };

                        var (subject, body) = await _templateService.RenderEmailTemplateAsync("citizen_welcome_registration", placeholders);
                        if (!string.IsNullOrEmpty(body))
                        {
                            await _emailSender.SendEmailAsync(Input.Email, subject, body);
                        }
                        else
                        {
                            await _emailSender.SendEmailAsync(Input.Email, "Konfirmo Email-in tënd - TeknoSOS",
                                $"Ju lutem konfirmoni llogarinë tuaj duke klikuar <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>këtu</a>.");
                        }
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        if (user.Role == UserRole.Professional)
                            return LocalRedirect(Url.Content("~/DefectList"));
                        else if (user.Role == UserRole.Citizen)
                            return LocalRedirect(Url.Content("~/ReportDefect"));
                        else if (user.Role == UserRole.Admin)
                            return LocalRedirect(Url.Content("~/Admin"));
                        else
                            return LocalRedirect(returnUrl);
                    }
                }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Gabim gjatë krijimit të llogarisë: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        /// <summary>
        /// Generates a display username in format "emer.mbiemer" from first and last name.
        /// Removes special characters, converts to lowercase, and handles Albanian characters.
        /// </summary>
        private string GenerateDisplayUsername(string firstName, string lastName)
        {
            // Normalize Albanian characters
            var normalizedFirst = NormalizeAlbanianText(firstName.Trim().ToLower());
            var normalizedLast = NormalizeAlbanianText(lastName.Trim().ToLower());
            
            // Remove any non-alphanumeric characters except dots
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

        /// <summary>
        /// Notify all admins when a new technician registers
        /// </summary>
        private async Task NotifyAdminsNewTechnician(ApplicationUser newTechnician)
        {
            try
            {
                // Get all admin users
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                
                var title = "Teknik i Ri u Regjistrua!";
                var message = $"{newTechnician.FirstName} {newTechnician.LastName} ({newTechnician.Email}) u regjistrua si teknik i ri. " +
                             $"Qyteti: {newTechnician.City ?? "I pa-specifikuar"}. Kërkon verifikim nga stafi.";

                // Create notification for each admin
                foreach (var admin in admins)
                {
                    var notification = new Notification
                    {
                        RecipientId = admin.Id,
                        ServiceRequestId = 0, // No service request associated
                        Type = NotificationType.NewTechnicianRegistered,
                        Title = title,
                        Message = message,
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);

                    // Send real-time SignalR notification to admin
                    await _notificationHub.Clients.Group($"user-{admin.Id}").SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        title = title,
                        message = message,
                        type = (int)NotificationType.NewTechnicianRegistered,
                        technicianId = newTechnician.Id,
                        technicianName = $"{newTechnician.FirstName} {newTechnician.LastName}",
                        technicianEmail = newTechnician.Email,
                        city = newTechnician.City,
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Admin notification sent for new technician: {Email}", newTechnician.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify admins about new technician: {Email}", newTechnician.Email);
                // Don't throw - registration should still succeed even if notification fails
            }
        }
    }
}
