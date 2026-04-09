using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PaymentsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISiteSettingsService _siteSettings;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;

        public PaymentsModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ISiteSettingsService siteSettings, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _siteSettings = siteSettings;
            _emailSender = emailSender;
        }

        public List<SubscriptionView> Subscriptions { get; set; } = new();
        public List<TechnicianSubView> Technicians { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingCount { get; set; }
        public int ActiveCount { get; set; }
        public int ExpiredCount { get; set; }

        [BindProperty] public string SubscriptionMonthlyAmount { get; set; } = "1999";
        [BindProperty] public string TechnicianMonthlyAmountALL { get; set; } = "1999";
        [BindProperty] public string BusinessMonthlyAmountALL { get; set; } = "3999";
        [BindProperty] public int MonthlyQuoteLimit { get; set; } = 20;
        [BindProperty] public string UnlimitedTechnicianIds { get; set; } = string.Empty;
        [BindProperty] public string UnlimitedBusinessIds { get; set; } = string.Empty;
        [BindProperty] public string StripeCheckoutUrl { get; set; } = string.Empty;
        [BindProperty] public string BktEposCheckoutUrl { get; set; } = string.Empty;
        [BindProperty] public string PayseraCheckoutUrl { get; set; } = string.Empty;
        [BindProperty] public string BankTransferIban { get; set; } = string.Empty;
        [BindProperty] public string BankTransferBeneficiary { get; set; } = string.Empty;
        [BindProperty] public string BankTransferSwift { get; set; } = string.Empty;
        [BindProperty] public string BillingSupportEmail { get; set; } = string.Empty;
        [BindProperty] public bool SubscriptionReminderEnabled { get; set; } = true;

        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        public class SubscriptionView
        {
            public int Id { get; set; }
            public string TechnicianName { get; set; } = "";
            public string TechnicianEmail { get; set; } = "";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; } = "";
            public bool IsConfirmed { get; set; }
            public bool IsTrialPeriod { get; set; }
            public DateTime? ConfirmedDate { get; set; }
            public string? AdminNotes { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class TechnicianSubView
        {
            public string Id { get; set; } = "";
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime RegistrationDate { get; set; }
            public DateTime? SubscriptionEndDate { get; set; }
            public bool IsPlatformActive { get; set; }
            public bool IsSubscriptionActive { get; set; }
            public bool IsInTrialPeriod { get; set; }
            public bool IsInRestrictedMode { get; set; }
            public int DaysRemaining { get; set; }
        }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"]?.ToString();

            var paymentSettings = await _siteSettings.GetGroupAsync("payments");
            SubscriptionMonthlyAmount = paymentSettings.TryGetValue("SubscriptionMonthlyAmount", out var monthly) ? monthly : "1999";
            TechnicianMonthlyAmountALL = paymentSettings.TryGetValue("TechnicianMonthlyAmountALL", out var techAmount) ? techAmount : "1999";
            BusinessMonthlyAmountALL = paymentSettings.TryGetValue("BusinessMonthlyAmountALL", out var bizAmount) ? bizAmount : "3999";
            MonthlyQuoteLimit = paymentSettings.TryGetValue("MonthlyQuoteLimit", out var quoteLimitRaw) && int.TryParse(quoteLimitRaw, out var parsedQuoteLimit)
                ? parsedQuoteLimit
                : 20;
            UnlimitedTechnicianIds = paymentSettings.TryGetValue("UnlimitedTechnicianIds", out var unlimitedTechRaw) ? unlimitedTechRaw : string.Empty;
            UnlimitedBusinessIds = paymentSettings.TryGetValue("UnlimitedBusinessIds", out var unlimitedBizRaw) ? unlimitedBizRaw : string.Empty;
            StripeCheckoutUrl = paymentSettings.TryGetValue("StripeCheckoutUrl", out var stripe) ? stripe : string.Empty;
            BktEposCheckoutUrl = paymentSettings.TryGetValue("BktEposCheckoutUrl", out var bkt) ? bkt : string.Empty;
            PayseraCheckoutUrl = paymentSettings.TryGetValue("PayseraCheckoutUrl", out var paysera) ? paysera : string.Empty;
            BankTransferIban = paymentSettings.TryGetValue("BankTransferIban", out var iban) ? iban : string.Empty;
            BankTransferBeneficiary = paymentSettings.TryGetValue("BankTransferBeneficiary", out var beneficiary) ? beneficiary : string.Empty;
            BankTransferSwift = paymentSettings.TryGetValue("BankTransferSwift", out var swift) ? swift : string.Empty;
            BillingSupportEmail = paymentSettings.TryGetValue("BillingSupportEmail", out var support) ? support : "billing@teknosos.app";
            SubscriptionReminderEnabled = !paymentSettings.TryGetValue("SubscriptionReminderEnabled", out var rem) || !string.Equals(rem, "false", StringComparison.OrdinalIgnoreCase);

            // Load all subscriptions
            var query = _db.TechnicianSubscriptions
                .Include(s => s.Technician)
                .OrderByDescending(s => s.CreatedDate)
                .AsQueryable();

            if (Filter == "pending") query = query.Where(s => !s.IsConfirmed && !s.IsTrialPeriod);
            else if (Filter == "confirmed") query = query.Where(s => s.IsConfirmed);
            else if (Filter == "trial") query = query.Where(s => s.IsTrialPeriod);

            Subscriptions = await query.Select(s => new SubscriptionView
            {
                Id = s.Id,
                TechnicianName = (s.Technician.FirstName ?? "") + " " + (s.Technician.LastName ?? ""),
                TechnicianEmail = s.Technician.Email ?? "",
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Amount = s.Amount,
                PaymentMethod = s.PaymentMethod ?? "",
                IsConfirmed = s.IsConfirmed,
                IsTrialPeriod = s.IsTrialPeriod,
                ConfirmedDate = s.ConfirmedDate,
                AdminNotes = s.AdminNotes ?? "",
                CreatedDate = s.CreatedDate
            }).ToListAsync();

            // KPIs
            TotalRevenue = await _db.TechnicianSubscriptions.Where(s => s.IsConfirmed).SumAsync(s => s.Amount);
            PendingCount = await _db.TechnicianSubscriptions.CountAsync(s => !s.IsConfirmed && !s.IsTrialPeriod);
            ActiveCount = await _db.TechnicianSubscriptions.CountAsync(s => s.IsConfirmed && s.EndDate > DateTime.UtcNow);
            ExpiredCount = await _db.TechnicianSubscriptions.CountAsync(s => s.EndDate < DateTime.UtcNow);

            // All technicians subscription status
            var proUsers = await _db.Users
                .IgnoreQueryFilters()
                .Where(u => u.Role == UserRole.Professional)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            Technicians = proUsers.Select(u => new TechnicianSubView
            {
                Id = u.Id,
                FullName = u.GetFullName(),
                Email = u.Email ?? "",
                RegistrationDate = u.RegistrationDate,
                SubscriptionEndDate = u.SubscriptionEndDate,
                IsPlatformActive = u.IsActive,
                IsSubscriptionActive = u.IsSubscriptionActive,
                IsInTrialPeriod = u.IsInTrialPeriod,
                IsInRestrictedMode = u.IsInRestrictedMode,
                DaysRemaining = u.IsSubscriptionActive
                    ? (u.SubscriptionEndDate.HasValue
                        ? (int)(u.SubscriptionEndDate.Value - DateTime.UtcNow).TotalDays
                        : (int)(u.RegistrationDate.AddDays(30) - DateTime.UtcNow).TotalDays)
                    : 0
            }).OrderBy(t => t.DaysRemaining).ToList();
        }

        public async Task<IActionResult> OnPostConfirmPaymentAsync(int subscriptionId)
        {
            var sub = await _db.TechnicianSubscriptions
                .Include(s => s.Technician)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (sub != null)
            {
                sub.IsConfirmed = true;
                sub.ConfirmedDate = DateTime.UtcNow;

                // Update technician subscription end date
                sub.Technician.SubscriptionEndDate = sub.EndDate;
                sub.Technician.IsActive = true;

                await _db.SaveChangesAsync();
                await TrySendSubscriptionChangeEmailAsync(
                    sub.Technician,
                    "Pagesa juaj u konfirmua",
                    $"Pagesa juaj për abonimin u konfirmua me sukses. Abonimi juaj është aktiv deri më <strong>{sub.EndDate:dd/MM/yyyy}</strong>.");
                TempData["Success"] = $"Pagesa #{sub.Id} për {sub.Technician.GetFullName()} u konfirmua.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectPaymentAsync(int subscriptionId, string? reason)
        {
            var sub = await _db.TechnicianSubscriptions
                .Include(s => s.Technician)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (sub != null)
            {
                sub.AdminNotes = $"Refuzuar: {reason ?? "Pa arsye"}";
                await _db.SaveChangesAsync();
                if (sub.Technician != null)
                {
                    await TrySendSubscriptionChangeEmailAsync(
                        sub.Technician,
                        "Pagesa juaj u refuzua",
                        $"Pagesa juaj për abonimin nuk u konfirmua. Arsyeja: <strong>{(string.IsNullOrWhiteSpace(reason) ? "Pa arsye të specifikuar" : reason)}</strong>.");
                }
                TempData["Success"] = $"Pagesa #{sub.Id} u refuzua.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExtendSubscriptionAsync(string technicianId, int months)
        {
            var user = await _userManager.FindByIdAsync(technicianId);
            if (user != null && months > 0)
            {
                var startDate = user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value > DateTime.UtcNow
                    ? user.SubscriptionEndDate.Value
                    : DateTime.UtcNow;
                var endDate = startDate.AddMonths(months);

                var subscription = new TechnicianSubscription
                {
                    TechnicianId = technicianId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Amount = 0,
                    PaymentMethod = "Admin-Manual",
                    IsTrialPeriod = false,
                    IsConfirmed = true,
                    ConfirmedDate = DateTime.UtcNow,
                    AdminNotes = $"Zgjatur manualisht nga admini për {months} muaj"
                };

                _db.TechnicianSubscriptions.Add(subscription);
                user.SubscriptionEndDate = endDate;
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                await _db.SaveChangesAsync();

                await TrySendSubscriptionChangeEmailAsync(
                    user,
                    "Abonimi juaj u zgjat",
                    $"Administratori zgjati abonimin tuaj me <strong>{months} muaj</strong>. Data e re e skadimit është <strong>{endDate:dd/MM/yyyy}</strong>.");
                TempData["Success"] = $"Abonimi i {user.GetFullName()} u zgjat deri më {endDate:dd/MM/yyyy}.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSuspendAsync(string technicianId)
        {
            var user = await _userManager.FindByIdAsync(technicianId);
            if (user != null)
            {
                user.SubscriptionEndDate = DateTime.UtcNow.AddDays(-1);
                user.IsActive = true;
                user.IsAvailableForWork = false;
                await _userManager.UpdateAsync(user);
                await TrySendSubscriptionChangeEmailAsync(
                    user,
                    "Abonimi juaj u pezullua",
                    "Administratori i platformës e ka pezulluar abonimin tuaj. Llogaria juaj mbetet e dukshme, por abonimi nuk është aktiv derisa të rinovohet.");
                TempData["Success"] = $"Abonimi i {user.GetFullName()} u pezullua.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveGatewaySettingsAsync()
        {
            var settings = new Dictionary<string, string>
            {
                ["SubscriptionMonthlyAmount"] = string.IsNullOrWhiteSpace(SubscriptionMonthlyAmount) ? "1999" : SubscriptionMonthlyAmount,
                ["TechnicianMonthlyAmountALL"] = string.IsNullOrWhiteSpace(TechnicianMonthlyAmountALL) ? "1999" : TechnicianMonthlyAmountALL,
                ["BusinessMonthlyAmountALL"] = string.IsNullOrWhiteSpace(BusinessMonthlyAmountALL) ? "3999" : BusinessMonthlyAmountALL,
                ["MonthlyQuoteLimit"] = MonthlyQuoteLimit <= 0 ? "20" : MonthlyQuoteLimit.ToString(),
                ["UnlimitedTechnicianIds"] = UnlimitedTechnicianIds?.Trim() ?? string.Empty,
                ["UnlimitedBusinessIds"] = UnlimitedBusinessIds?.Trim() ?? string.Empty,
                ["StripeCheckoutUrl"] = StripeCheckoutUrl?.Trim() ?? string.Empty,
                ["BktEposCheckoutUrl"] = BktEposCheckoutUrl?.Trim() ?? string.Empty,
                ["PayseraCheckoutUrl"] = PayseraCheckoutUrl?.Trim() ?? string.Empty,
                ["BankTransferIban"] = BankTransferIban?.Trim() ?? string.Empty,
                ["BankTransferBeneficiary"] = BankTransferBeneficiary?.Trim() ?? string.Empty,
                ["BankTransferSwift"] = BankTransferSwift?.Trim() ?? string.Empty,
                ["BillingSupportEmail"] = BillingSupportEmail?.Trim() ?? string.Empty,
                ["SubscriptionReminderEnabled"] = SubscriptionReminderEnabled ? "true" : "false"
            };

            await _siteSettings.SetGroupAsync("payments", settings);
            TempData["Success"] = "Konfigurimet e pagesave u ruajtën me sukses.";
            return RedirectToPage();
        }

        private async Task TrySendSubscriptionChangeEmailAsync(ApplicationUser user, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                return;

            try
            {
                var fullName = string.IsNullOrWhiteSpace(user.GetFullName()) ? user.Email : user.GetFullName();
                var body = $@"<html><body style='font-family:Arial,sans-serif;line-height:1.6;'>
<p>Përshëndetje {fullName},</p>
<p>{message}</p>
<p>Për çdo pyetje mbi abonimin tuaj, ju lutem kontaktoni administratorin e TeknoSOS.</p>
<p>Faleminderit,<br/>TeknoSOS</p>
</body></html>";

                await _emailSender.SendEmailAsync(user.Email, subject, body);
            }
            catch
            {
                // Notification email failure should not block payment/subscription changes.
            }
        }
    }
}
