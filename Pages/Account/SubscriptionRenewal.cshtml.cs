using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Account;

[Authorize]
public class SubscriptionRenewalModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly ISiteSettingsService _settings;

    public SubscriptionRenewalModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        ISiteSettingsService settings)
    {
        _userManager = userManager;
        _db = db;
        _settings = settings;
    }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DaysLeft { get; set; }
    public bool IsExpired { get; set; }
    public decimal MonthlyAmount { get; set; }

    [BindProperty]
    public RenewalInput Input { get; set; } = new();

    public class RenewalInput
    {
        [Required]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10,16}$", ErrorMessage = "NID/NIPT duhet te kete 10 deri 16 shifra.")]
        public string NidOrNipt { get; set; } = string.Empty;

        [Range(1, 12)]
        public int Months { get; set; } = 1;

        [Required]
        public string PaymentGateway { get; set; } = "Stripe";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        if (user.Role != UserRole.Professional)
            return RedirectToPage("/Dashboard");

        var subscriptionEnd = user.SubscriptionEndDate ?? user.RegistrationDate.AddDays(90);
        DaysLeft = Math.Max(0, (int)Math.Ceiling((subscriptionEnd - DateTime.UtcNow).TotalDays));
        IsExpired = subscriptionEnd <= DateTime.UtcNow;

        MonthlyAmount = await GetMonthlyAmountAsync();

        FullName = user.GetFullName();
        Email = user.Email ?? string.Empty;

        Input.FullName = FullName;
        Input.Email = Email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        if (user.Role != UserRole.Professional)
            return RedirectToPage("/Dashboard");

        MonthlyAmount = await GetMonthlyAmountAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var allowedGateways = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Stripe", "BKT-EPOS", "Paysera", "BankTransfer"
        };

        if (!allowedGateways.Contains(Input.PaymentGateway))
        {
            ModelState.AddModelError(string.Empty, "Porta e pageses nuk mbeshtetet.");
            return Page();
        }

        var startDate = user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value > DateTime.UtcNow
            ? user.SubscriptionEndDate.Value
            : DateTime.UtcNow;

        var endDate = startDate.AddMonths(Input.Months);
        var amount = MonthlyAmount * Input.Months;

        var request = new TechnicianSubscription
        {
            TechnicianId = user.Id,
            StartDate = startDate,
            EndDate = endDate,
            Amount = amount,
            PaymentMethod = Input.PaymentGateway,
            IsTrialPeriod = false,
            IsConfirmed = false,
            AdminNotes = $"Renewal request | Name: {Input.FullName} | Email: {Input.Email} | NID/NIPT: {Input.NidOrNipt} | Months: {Input.Months} | Notes: {Input.Notes}",
            CreatedDate = DateTime.UtcNow
        };

        _db.TechnicianSubscriptions.Add(request);
        await _db.SaveChangesAsync();

        var paymentSettings = await _settings.GetGroupAsync("payments");
        var targetUrl = Input.PaymentGateway switch
        {
            "Stripe" => paymentSettings.TryGetValue("StripeCheckoutUrl", out var stripe) ? stripe : string.Empty,
            "BKT-EPOS" => paymentSettings.TryGetValue("BktEposCheckoutUrl", out var bkt) ? bkt : string.Empty,
            "Paysera" => paymentSettings.TryGetValue("PayseraCheckoutUrl", out var paysera) ? paysera : string.Empty,
            _ => string.Empty
        };

        TempData["SubscriptionMessage"] = "Kerkesa u regjistrua me sukses. Admini do ta shqyrtoje dhe/ose do te ridrejtoheni te pagesa.";

        if (!string.IsNullOrWhiteSpace(targetUrl))
        {
            return Redirect(targetUrl);
        }

        return RedirectToPage("/Dashboard");
    }

    private async Task<decimal> GetMonthlyAmountAsync()
    {
        var amountRaw = await _settings.GetAsync("payments", "TechnicianMonthlyAmountALL", "1999");
        return decimal.TryParse(amountRaw, out var parsed) ? parsed : 1999m;
    }
}
