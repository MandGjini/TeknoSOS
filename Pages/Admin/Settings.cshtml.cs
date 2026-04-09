using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SettingsModel : PageModel
    {
        private readonly ISiteSettingsService _settings;

        public SettingsModel(ISiteSettingsService settings)
        {
            _settings = settings;
        }

        public Dictionary<string, string> Branding { get; set; } = new();
        public Dictionary<string, string> Contact { get; set; } = new();
        public Dictionary<string, string> Seo { get; set; } = new();
        public Dictionary<string, string> Social { get; set; } = new();
        public Dictionary<string, string> Languages { get; set; } = new();
        public Dictionary<string, string> General { get; set; } = new();
        public Dictionary<string, string> Footer { get; set; } = new();
        public Dictionary<string, string> Analytics { get; set; } = new();
        public Dictionary<string, string> Adsense { get; set; } = new();
        public string? SuccessMsg { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMsg = TempData["Success"]?.ToString();
            Branding = await _settings.GetGroupAsync("branding");
            Contact = await _settings.GetGroupAsync("contact");
            Seo = await _settings.GetGroupAsync("seo");
            Social = await _settings.GetGroupAsync("social");
            Languages = await _settings.GetGroupAsync("languages");
            General = await _settings.GetGroupAsync("general");
            Footer = await _settings.GetGroupAsync("footer");
            Analytics = await _settings.GetGroupAsync("analytics");
            Adsense = await _settings.GetGroupAsync("adsense");
        }

        private string GetVal(Dictionary<string, string> dict, string key, string def = "")
            => dict.TryGetValue(key, out var v) ? v : def;

        public async Task<IActionResult> OnPostBrandingAsync(string siteName, string siteTagline, string primaryColor, string secondaryColor, string logoUrl, string faviconUrl)
        {
            await _settings.SetGroupAsync("branding", new Dictionary<string, string>
            {
                ["SiteName"] = siteName ?? "TeknoSOS",
                ["SiteTagline"] = siteTagline ?? "",
                ["PrimaryColor"] = primaryColor ?? "#2563eb",
                ["SecondaryColor"] = secondaryColor ?? "#764ba2",
                ["LogoUrl"] = logoUrl ?? "/images/logo.png",
                ["FaviconUrl"] = faviconUrl ?? "/favicon.ico"
            });
            TempData["Success"] = "Branding u perditesua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostContactAsync(string contactEmail, string contactPhone, string contactAddress)
        {
            await _settings.SetGroupAsync("contact", new Dictionary<string, string>
            {
                ["ContactEmail"] = contactEmail ?? "",
                ["ContactPhone"] = contactPhone ?? "",
                ["ContactAddress"] = contactAddress ?? ""
            });
            TempData["Success"] = "Informacionet e kontaktit u perditesuan.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSeoAsync(string metaDescription, string metaKeywords)
        {
            await _settings.SetGroupAsync("seo", new Dictionary<string, string>
            {
                ["MetaDescription"] = metaDescription ?? "",
                ["MetaKeywords"] = metaKeywords ?? ""
            });
            TempData["Success"] = "Cilesimet SEO u perditesuan.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSeoAdvancedAsync(string siteBaseUrl)
        {
            var seo = await _settings.GetGroupAsync("seo");
            seo["SiteBaseUrl"] = string.IsNullOrWhiteSpace(siteBaseUrl) ? "https://teknosos.app" : siteBaseUrl.TrimEnd('/');
            await _settings.SetGroupAsync("seo", seo);

            TempData["Success"] = "SEO Advanced u përditësua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSocialAsync(string facebook, string instagram, string linkedin, string twitter)
        {
            await _settings.SetGroupAsync("social", new Dictionary<string, string>
            {
                ["Facebook"] = facebook ?? "",
                ["Instagram"] = instagram ?? "",
                ["LinkedIn"] = linkedin ?? "",
                ["Twitter"] = twitter ?? ""
            });
            TempData["Success"] = "Rrjetet sociale u perditesuan.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLanguagesAsync(string defaultLanguage, string activeLanguages)
        {
            await _settings.SetGroupAsync("languages", new Dictionary<string, string>
            {
                ["DefaultLanguage"] = defaultLanguage ?? "sq",
                ["ActiveLanguages"] = activeLanguages ?? "sq,en,it,de,fr"
            });
            TempData["Success"] = "Cilesimet e gjuheve u perditesuan.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostGeneralAsync(string maintenanceMode, string allowRegistration, string requireEmailConfirmation)
        {
            await _settings.SetGroupAsync("general", new Dictionary<string, string>
            {
                ["MaintenanceMode"] = maintenanceMode ?? "false",
                ["AllowRegistration"] = allowRegistration ?? "true",
                ["RequireEmailConfirmation"] = requireEmailConfirmation ?? "false"
            });
            TempData["Success"] = "Cilesimet e pergjithshme u perditesuan.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFooterAsync(
            string footerDescription, 
            string footerAddress, 
            string footerEmail, 
            string footerPhone,
            string footerFacebook,
            string footerInstagram,
            string footerLinkedIn,
            string footerTwitter,
            string footerCopyright,
            string footerMadeWith)
        {
            await _settings.SetGroupAsync("footer", new Dictionary<string, string>
            {
                ["Description"] = footerDescription ?? "Platforma e besueshme për të gjitha shërbimet e shtëpisë.",
                ["Address"] = footerAddress ?? "Tiranë, Shqipëri",
                ["Email"] = footerEmail ?? "info@teknosos.al",
                ["Phone"] = footerPhone ?? "+355 69 123 4567",
                ["Facebook"] = footerFacebook ?? "#",
                ["Instagram"] = footerInstagram ?? "#",
                ["LinkedIn"] = footerLinkedIn ?? "#",
                ["Twitter"] = footerTwitter ?? "#",
                ["Copyright"] = footerCopyright ?? "TeknoSOS",
                ["MadeWith"] = footerMadeWith ?? "Made with ❤️ in Albania"
            });
            TempData["Success"] = "Cilesimet e Footer u perditesuan me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAnalyticsAsync(string ga4MeasurementId, string gtmContainerId, string enableTracking)
        {
            await _settings.SetGroupAsync("analytics", new Dictionary<string, string>
            {
                ["GA4MeasurementId"] = ga4MeasurementId ?? "",
                ["GTMContainerId"] = gtmContainerId ?? "",
                ["EnableTracking"] = enableTracking ?? "false"
            });
            TempData["Success"] = "Cilësimet e Analytics u përditësuan me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAdsenseAsync(
            string adsenseEnabled,
            string adsenseClientId,
            string blogListTopSlot,
            string blogListInFeedSlot,
            string blogArticleTopSlot,
            string blogArticleInlineSlot,
            string blogArticleBottomSlot,
            string maxInlineAdsPerArticle)
        {
            await _settings.SetGroupAsync("adsense", new Dictionary<string, string>
            {
                ["Enabled"] = adsenseEnabled ?? "false",
                ["ClientId"] = adsenseClientId ?? "",
                ["BlogListTopSlot"] = blogListTopSlot ?? "",
                ["BlogListInFeedSlot"] = blogListInFeedSlot ?? "",
                ["BlogArticleTopSlot"] = blogArticleTopSlot ?? "",
                ["BlogArticleInlineSlot"] = blogArticleInlineSlot ?? "",
                ["BlogArticleBottomSlot"] = blogArticleBottomSlot ?? "",
                ["MaxInlineAdsPerArticle"] = maxInlineAdsPerArticle ?? "2"
            });

            TempData["Success"] = "Cilësimet e AdSense u përditësuan me sukses.";
            return RedirectToPage();
        }
    }
}
