using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class LanguageManagerModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ISiteContentService _contentService;
        private readonly ILocalizationService _localizer;

        public LanguageManagerModel(ApplicationDbContext context, ISiteContentService contentService, ILocalizationService localizer)
        {
            _context = context;
            _contentService = contentService;
            _localizer = localizer;
        }

        public List<LanguageStat> LanguageStats { get; set; } = new();
        public int TotalContentBlocks { get; set; }
        public List<string> MissingTranslations { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? CheckLanguage { get; set; }

        public class LanguageStat
        {
            public string LanguageCode { get; set; } = string.Empty;
            public string LanguageName { get; set; } = string.Empty;
            public int ContentCount { get; set; }
            public int MissingCount { get; set; }
            public double CompletionPercent { get; set; }
        }

        public async Task OnGetAsync()
        {
            var allContent = await _context.SiteContents.ToListAsync();
            TotalContentBlocks = allContent.Count;

            var languages = new Dictionary<string, string>
            {
                { "sq", "Shqip" },
                { "en", "English" },
                { "it", "Italiano" },
                { "de", "Deutsch" },
                { "fr", "Français" }
            };

            // Get all page+section keys from the default language (sq)
            var sqKeys = allContent
                .Where(c => c.Language == "sq")
                .Select(c => $"{c.PageKey}:{c.SectionKey}")
                .Distinct()
                .ToList();

            var totalKeys = sqKeys.Count;

            foreach (var (code, name) in languages)
            {
                var langKeys = allContent
                    .Where(c => c.Language == code)
                    .Select(c => $"{c.PageKey}:{c.SectionKey}")
                    .Distinct()
                    .ToList();

                var count = langKeys.Count;
                var missing = totalKeys > 0 ? totalKeys - count : 0;

                LanguageStats.Add(new LanguageStat
                {
                    LanguageCode = code,
                    LanguageName = name,
                    ContentCount = count,
                    MissingCount = missing > 0 ? missing : 0,
                    CompletionPercent = totalKeys > 0 ? Math.Round((double)count / totalKeys * 100, 1) : 0
                });
            }

            // Find missing translations for the selected language
            if (!string.IsNullOrEmpty(CheckLanguage) && CheckLanguage != "sq")
            {
                var langKeys = allContent
                    .Where(c => c.Language == CheckLanguage)
                    .Select(c => $"{c.PageKey}:{c.SectionKey}")
                    .ToHashSet();

                MissingTranslations = sqKeys.Where(k => !langKeys.Contains(k)).ToList();
            }
        }

        public async Task<IActionResult> OnPostAutoTranslateAsync(string language)
        {
            // Auto-duplicate sq content as a starting point for translation
            var sqContent = await _context.SiteContents
                .Where(c => c.Language == "sq")
                .ToListAsync();

            var existingKeys = await _context.SiteContents
                .Where(c => c.Language == language)
                .Select(c => $"{c.PageKey}:{c.SectionKey}")
                .ToListAsync();

            var existingSet = new HashSet<string>(existingKeys);
            int added = 0;

            foreach (var content in sqContent)
            {
                var key = $"{content.PageKey}:{content.SectionKey}";
                if (!existingSet.Contains(key))
                {
                    _context.SiteContents.Add(new SiteContent
                    {
                        PageKey = content.PageKey,
                        SectionKey = content.SectionKey,
                        ContentType = content.ContentType,
                        Title = content.Title != null ? $"[{language.ToUpper()}] {content.Title}" : null,
                        Content = content.Content != null ? $"[{language.ToUpper()}] {content.Content}" : "",
                        Language = language,
                        SortOrder = content.SortOrder,
                        IsActive = content.IsActive,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = "admin-auto"
                    });
                    added++;
                }
            }

            if (added > 0)
                await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"U shtuan {added} blloqe permbajtje per {language.ToUpper()}. Editoni permbajtjen ne Content Manager.";
            return RedirectToPage(new { CheckLanguage = language });
        }
    }
}
