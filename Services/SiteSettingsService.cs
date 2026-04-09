using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public interface ISiteSettingsService
    {
        Task<string> GetAsync(string groupKey, string settingKey, string defaultValue = "");
        Task<Dictionary<string, string>> GetGroupAsync(string groupKey);
        Task<Dictionary<string, Dictionary<string, string>>> GetAllAsync();
        Task SetAsync(string groupKey, string settingKey, string value);
        Task SetGroupAsync(string groupKey, Dictionary<string, string> settings);
        Task<List<SiteSetting>> GetAllSettingsAsync();
        Task SeedDefaultsAsync();
        void InvalidateCache(string? groupKey = null);
    }

    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CacheKeyPrefix = "SiteSetting_";
        private const string AllSettingsCacheKey = "SiteSettings_All";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public SiteSettingsService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<string> GetAsync(string groupKey, string settingKey, string defaultValue = "")
        {
            var cacheKey = $"{CacheKeyPrefix}{groupKey}_{settingKey}";
            
            if (_cache.TryGetValue(cacheKey, out string? cachedValue))
                return cachedValue ?? defaultValue;

            var setting = await _db.SiteSettings
                .Where(s => s.GroupKey == groupKey && s.SettingKey == settingKey)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();
            
            var value = setting ?? defaultValue;
            _cache.Set(cacheKey, value, CacheDuration);
            
            return value;
        }

        public async Task<Dictionary<string, string>> GetGroupAsync(string groupKey)
        {
            var cacheKey = $"{CacheKeyPrefix}Group_{groupKey}";
            
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedGroup))
                return cachedGroup ?? new Dictionary<string, string>();

            var group = await _db.SiteSettings
                .Where(s => s.GroupKey == groupKey)
                .OrderBy(s => s.SortOrder)
                .ToDictionaryAsync(s => s.SettingKey, s => s.Value);
            
            _cache.Set(cacheKey, group, CacheDuration);
            return group;
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllAsync()
        {
            if (_cache.TryGetValue(AllSettingsCacheKey, out Dictionary<string, Dictionary<string, string>>? cached))
                return cached ?? new Dictionary<string, Dictionary<string, string>>();

            var settings = await _db.SiteSettings
                .OrderBy(s => s.GroupKey).ThenBy(s => s.SortOrder)
                .ToListAsync();

            var result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var setting in settings)
            {
                if (!result.ContainsKey(setting.GroupKey))
                    result[setting.GroupKey] = new Dictionary<string, string>();
                result[setting.GroupKey][setting.SettingKey] = setting.Value;
            }
            
            _cache.Set(AllSettingsCacheKey, result, CacheDuration);
            return result;
        }

        public async Task SetAsync(string groupKey, string settingKey, string value)
        {
            var setting = await _db.SiteSettings
                .FirstOrDefaultAsync(s => s.GroupKey == groupKey && s.SettingKey == settingKey);
            if (setting != null)
            {
                setting.Value = value;
                setting.LastModifiedDate = DateTime.UtcNow;
            }
            else
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    GroupKey = groupKey,
                    SettingKey = settingKey,
                    Value = value,
                    LastModifiedDate = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
            
            // Invalidate cache
            InvalidateCache(groupKey);
        }

        public async Task SetGroupAsync(string groupKey, Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                var existing = await _db.SiteSettings
                    .FirstOrDefaultAsync(s => s.GroupKey == groupKey && s.SettingKey == kvp.Key);
                if (existing != null)
                {
                    existing.Value = kvp.Value;
                    existing.LastModifiedDate = DateTime.UtcNow;
                }
                else
                {
                    _db.SiteSettings.Add(new SiteSetting
                    {
                        GroupKey = groupKey,
                        SettingKey = kvp.Key,
                        Value = kvp.Value,
                        LastModifiedDate = DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();
            
            // Invalidate cache for this group
            InvalidateCache(groupKey);
        }

        public async Task<List<SiteSetting>> GetAllSettingsAsync()
        {
            return await _db.SiteSettings
                .OrderBy(s => s.GroupKey).ThenBy(s => s.SortOrder)
                .ToListAsync();
        }

        public void InvalidateCache(string? groupKey = null)
        {
            _cache.Remove(AllSettingsCacheKey);
            
            if (!string.IsNullOrEmpty(groupKey))
            {
                _cache.Remove($"{CacheKeyPrefix}Group_{groupKey}");
            }
        }

        public async Task SeedDefaultsAsync()
        {
            if (await _db.SiteSettings.AnyAsync()) return;

            var defaults = new List<SiteSetting>
            {
                // Branding
                new() { GroupKey = "branding", SettingKey = "SiteName", Value = "TeknoSOS", DisplayLabel = "Emri i platformes", InputType = "text", SortOrder = 1 },
                new() { GroupKey = "branding", SettingKey = "SiteTagline", Value = "Platforma e sherbimeve teknike", DisplayLabel = "Slogani", InputType = "text", SortOrder = 2 },
                new() { GroupKey = "branding", SettingKey = "PrimaryColor", Value = "#2563eb", DisplayLabel = "Ngjyra primare", InputType = "color", SortOrder = 3 },
                new() { GroupKey = "branding", SettingKey = "SecondaryColor", Value = "#764ba2", DisplayLabel = "Ngjyra sekondare", InputType = "color", SortOrder = 4 },
                new() { GroupKey = "branding", SettingKey = "LogoUrl", Value = "/images/logo.png", DisplayLabel = "URL e logos", InputType = "text", SortOrder = 5 },
                new() { GroupKey = "branding", SettingKey = "FaviconUrl", Value = "/favicon.ico", DisplayLabel = "URL e favicon", InputType = "text", SortOrder = 6 },

                // Contact
                new() { GroupKey = "contact", SettingKey = "ContactEmail", Value = "info@teknosos.app", DisplayLabel = "Email kontakti", InputType = "email", SortOrder = 1 },
                new() { GroupKey = "contact", SettingKey = "ContactPhone", Value = "+355 69 344 6516", DisplayLabel = "Telefoni", InputType = "text", SortOrder = 2 },
                new() { GroupKey = "contact", SettingKey = "SOSPhone", Value = "00355693446516", DisplayLabel = "Numri SOS Emergjence", InputType = "text", SortOrder = 3 },
                new() { GroupKey = "contact", SettingKey = "ContactAddress", Value = "Tirane, Shqiperi", DisplayLabel = "Adresa", InputType = "text", SortOrder = 4 },

                // SEO
                new() { GroupKey = "seo", SettingKey = "MetaDescription", Value = "TeknoSOS - Platforma per raportimin dhe menaxhimin e defekteve teknike.", DisplayLabel = "Meta pershkrimi", InputType = "textarea", SortOrder = 1 },
                new() { GroupKey = "seo", SettingKey = "MetaKeywords", Value = "teknosos, sherbime teknike, riparime, hidraulik, elektricist", DisplayLabel = "Meta fjalet kyqe", InputType = "textarea", SortOrder = 2 },
                new() { GroupKey = "seo", SettingKey = "SiteBaseUrl", Value = "https://teknosos.app", DisplayLabel = "Site Base URL", InputType = "url", SortOrder = 3 },

                // Social
                new() { GroupKey = "social", SettingKey = "Facebook", Value = "", DisplayLabel = "Facebook URL", InputType = "url", SortOrder = 1 },
                new() { GroupKey = "social", SettingKey = "Instagram", Value = "", DisplayLabel = "Instagram URL", InputType = "url", SortOrder = 2 },
                new() { GroupKey = "social", SettingKey = "LinkedIn", Value = "", DisplayLabel = "LinkedIn URL", InputType = "url", SortOrder = 3 },
                new() { GroupKey = "social", SettingKey = "Twitter", Value = "", DisplayLabel = "Twitter URL", InputType = "url", SortOrder = 4 },

                // Languages
                new() { GroupKey = "languages", SettingKey = "DefaultLanguage", Value = "sq", DisplayLabel = "Gjuha e parazgjedhur", InputType = "select", SelectOptions = "sq,en,it,de,fr", SortOrder = 1 },
                new() { GroupKey = "languages", SettingKey = "ActiveLanguages", Value = "sq,en,it,de,fr", DisplayLabel = "Gjuhet aktive", InputType = "text", SortOrder = 2 },

                // Analytics
                new() { GroupKey = "analytics", SettingKey = "GA4MeasurementId", Value = "G-VR3ZX35D8B", DisplayLabel = "GA4 Measurement ID", InputType = "text", SortOrder = 1 },
                new() { GroupKey = "analytics", SettingKey = "GTMContainerId", Value = "GTM-MNB34J4N", DisplayLabel = "GTM Container ID", InputType = "text", SortOrder = 2 },
                new() { GroupKey = "analytics", SettingKey = "EnableTracking", Value = "true", DisplayLabel = "Aktivizo gjurmimin", InputType = "toggle", SortOrder = 3 },

                // General
                new() { GroupKey = "general", SettingKey = "MaintenanceMode", Value = "false", DisplayLabel = "Modaliteti i mirembajtjes", InputType = "toggle", SortOrder = 1 },
                new() { GroupKey = "general", SettingKey = "AllowRegistration", Value = "true", DisplayLabel = "Lejo regjistrimin", InputType = "toggle", SortOrder = 2 },
                new() { GroupKey = "general", SettingKey = "RequireEmailConfirmation", Value = "false", DisplayLabel = "Kerko konfirmimin e emailit", InputType = "toggle", SortOrder = 3 },
                new() { GroupKey = "general", SettingKey = "SetupCompleted", Value = "false", DisplayLabel = "Konfigurimi i plotesuar", InputType = "toggle", SortOrder = 10 },

                // Payments
                new() { GroupKey = "payments", SettingKey = "SubscriptionMonthlyAmount", Value = "1999", DisplayLabel = "Tarifa mujore e abonimit (ALL)", InputType = "text", SortOrder = 1 },
                new() { GroupKey = "payments", SettingKey = "TechnicianMonthlyAmountALL", Value = "1999", DisplayLabel = "Tarifa mujore per tekniket (ALL)", InputType = "text", SortOrder = 2 },
                new() { GroupKey = "payments", SettingKey = "BusinessMonthlyAmountALL", Value = "3999", DisplayLabel = "Tarifa mujore per bizneset (ALL)", InputType = "text", SortOrder = 3 },
                new() { GroupKey = "payments", SettingKey = "MonthlyQuoteLimit", Value = "20", DisplayLabel = "Kufiri mujor i preventivave", InputType = "number", SortOrder = 4 },
                new() { GroupKey = "payments", SettingKey = "UnlimitedTechnicianIds", Value = "", DisplayLabel = "Teknike pa limit (IDs)", InputType = "textarea", SortOrder = 5 },
                new() { GroupKey = "payments", SettingKey = "UnlimitedBusinessIds", Value = "", DisplayLabel = "Biznese pa limit (IDs)", InputType = "textarea", SortOrder = 6 },
                new() { GroupKey = "payments", SettingKey = "StripeCheckoutUrl", Value = "", DisplayLabel = "Stripe Checkout URL", InputType = "url", SortOrder = 7 },
                new() { GroupKey = "payments", SettingKey = "BktEposCheckoutUrl", Value = "", DisplayLabel = "BKT ePOS URL", InputType = "url", SortOrder = 8 },
                new() { GroupKey = "payments", SettingKey = "PayseraCheckoutUrl", Value = "", DisplayLabel = "Paysera URL", InputType = "url", SortOrder = 9 },
                new() { GroupKey = "payments", SettingKey = "BankTransferIban", Value = "", DisplayLabel = "IBAN", InputType = "text", SortOrder = 10 },
                new() { GroupKey = "payments", SettingKey = "BankTransferBeneficiary", Value = "", DisplayLabel = "Përfituesi", InputType = "text", SortOrder = 11 },
                new() { GroupKey = "payments", SettingKey = "BankTransferSwift", Value = "", DisplayLabel = "SWIFT", InputType = "text", SortOrder = 12 },
                new() { GroupKey = "payments", SettingKey = "BillingSupportEmail", Value = "billing@teknosos.app", DisplayLabel = "Billing Support Email", InputType = "email", SortOrder = 13 },
                new() { GroupKey = "payments", SettingKey = "SubscriptionReminderEnabled", Value = "true", DisplayLabel = "Aktivizo rikujtimet e abonimit", InputType = "toggle", SortOrder = 14 },

                // AdSense
                new() { GroupKey = "adsense", SettingKey = "Enabled", Value = "false", DisplayLabel = "Aktivizo AdSense", InputType = "toggle", SortOrder = 1 },
                new() { GroupKey = "adsense", SettingKey = "ClientId", Value = "", DisplayLabel = "AdSense Client ID", InputType = "text", SortOrder = 2 },
                new() { GroupKey = "adsense", SettingKey = "BlogListTopSlot", Value = "", DisplayLabel = "Blog List Top Slot", InputType = "text", SortOrder = 3 },
                new() { GroupKey = "adsense", SettingKey = "BlogListInFeedSlot", Value = "", DisplayLabel = "Blog List In-Feed Slot", InputType = "text", SortOrder = 4 },
                new() { GroupKey = "adsense", SettingKey = "BlogArticleTopSlot", Value = "", DisplayLabel = "Blog Article Top Slot", InputType = "text", SortOrder = 5 },
                new() { GroupKey = "adsense", SettingKey = "BlogArticleInlineSlot", Value = "", DisplayLabel = "Blog Article Inline Slot", InputType = "text", SortOrder = 6 },
                new() { GroupKey = "adsense", SettingKey = "BlogArticleBottomSlot", Value = "", DisplayLabel = "Blog Article Bottom Slot", InputType = "text", SortOrder = 7 },
                new() { GroupKey = "adsense", SettingKey = "MaxInlineAdsPerArticle", Value = "2", DisplayLabel = "Max Inline Ads per Article", InputType = "number", SortOrder = 8 },
            };

            _db.SiteSettings.AddRange(defaults);
            await _db.SaveChangesAsync();
        }
    }
}
