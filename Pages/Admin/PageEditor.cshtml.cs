using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PageEditorModel : PageModel
    {
        private readonly ISiteContentService _cms;

        public PageEditorModel(ISiteContentService cms)
        {
            _cms = cms;
        }

        [BindProperty(SupportsGet = true)]
        public string? PageKey { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Language { get; set; } = "sq";

        public List<string> AvailablePages { get; set; } = new();
        public SiteContent? PageContent { get; set; }
        public string? SuccessMsg { get; set; }

        // List of editable pages with display names
        public static readonly Dictionary<string, string> EditablePages = new()
        {
            { "home", "Faqja Kryesore" },
            { "about", "Rreth Nesh" },
            { "howitworks", "Si Funksionon" },
            { "faq", "FAQ - Pyetje & Përgjigje" },
            { "contact", "Kontakt" },
            { "terms", "Kushtet e Shërbimit" },
            { "privacy", "Politika e Privatësisë" },
            { "cookies", "Politika e Cookies" },
            { "disclaimer", "Disclaimer" },
            { "dataprocessing", "Përpunimi i të Dhënave" },
            { "technicians", "Lista e Teknikëve" }
        };

        public async Task OnGetAsync()
        {
            SuccessMsg = TempData["Success"]?.ToString();
            Language ??= "sq";
            
            AvailablePages = await _cms.GetDistinctPagesAsync();
            
            // If page selected, load existing content
            if (!string.IsNullOrEmpty(PageKey))
            {
                PageContent = await _cms.GetBlockAsync(PageKey, "full_page", Language);
            }
        }

        public async Task<IActionResult> OnPostSavePageAsync(string pageKey, string language, string htmlContent, string cssContent, string gjsComponents, string gjsStyles)
        {
            if (string.IsNullOrEmpty(pageKey))
            {
                return BadRequest("Page key is required");
            }

            // Save/update the full page content
            var existingContent = await _cms.GetBlockAsync(pageKey, "full_page", language ?? "sq");
            
            var content = existingContent ?? new SiteContent
            {
                PageKey = pageKey,
                SectionKey = "full_page",
                Language = language ?? "sq",
                ContentType = "html",
                IsActive = true
            };

            content.Content = htmlContent ?? "";
            content.Metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                css = cssContent ?? "",
                gjsComponents = gjsComponents ?? "",
                gjsStyles = gjsStyles ?? ""
            });
            content.LastModifiedDate = DateTime.UtcNow;
            content.LastModifiedBy = User.Identity?.Name;

            await _cms.SaveContentAsync(content, User.Identity?.Name);

            TempData["Success"] = $"Faqja '{pageKey}' u ruajt me sukses!";
            return RedirectToPage(new { PageKey = pageKey, Language = language });
        }

        public async Task<IActionResult> OnGetLoadPageAsync(string pageKey, string language)
        {
            var lang = language ?? "sq";
            // First check if there's a full_page visual builder block
            var content = await _cms.GetBlockAsync(pageKey, "full_page", lang);
            
            if (content != null)
            {
                return new JsonResult(new
                {
                    html = content.Content,
                    css = GetMetadataValue(content.Metadata, "css"),
                    gjsComponents = GetMetadataValue(content.Metadata, "gjsComponents"),
                    gjsStyles = GetMetadataValue(content.Metadata, "gjsStyles")
                });
            }

            // No full_page block — assemble from individual content sections
            var sections = await _cms.GetPageContentAsync(pageKey, lang);
            if (sections == null || !sections.Any())
            {
                return new JsonResult(new { html = "", css = "", gjsComponents = "", gjsStyles = "" });
            }

            var sb = new System.Text.StringBuilder();
            foreach (var s in sections)
            {
                if (s.ContentType == "html")
                {
                    sb.AppendLine($"<section class=\"py-4\"><div class=\"container\">");
                    if (!string.IsNullOrEmpty(s.Title))
                        sb.AppendLine($"<h2 class=\"fw-bold mb-3\">{System.Net.WebUtility.HtmlEncode(s.Title)}</h2>");
                    sb.AppendLine(s.Content);
                    sb.AppendLine("</div></section>");
                }
                else if (s.ContentType == "text")
                {
                    var tag = s.SectionKey.Contains("title") ? "h2" : "p";
                    sb.AppendLine($"<{tag} data-section=\"{System.Net.WebUtility.HtmlEncode(s.SectionKey)}\">{System.Net.WebUtility.HtmlEncode(s.Content)}</{tag}>");
                }
                else if (s.ContentType == "feature" || s.ContentType == "step" || s.ContentType == "stat" || s.ContentType == "testimonial" || s.ContentType == "faq")
                {
                    sb.AppendLine($"<div class=\"card border-0 shadow-sm mb-3 p-3\" data-section=\"{System.Net.WebUtility.HtmlEncode(s.SectionKey)}\" data-type=\"{s.ContentType}\">");
                    if (!string.IsNullOrEmpty(s.IconClass))
                        sb.AppendLine($"<i class=\"bi {System.Net.WebUtility.HtmlEncode(s.IconClass)} mb-2\" style=\"font-size:1.5rem;color:#F28C28;\"></i>");
                    if (!string.IsNullOrEmpty(s.Title))
                        sb.AppendLine($"<h5 class=\"fw-bold\">{System.Net.WebUtility.HtmlEncode(s.Title)}</h5>");
                    sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(s.Content)}</p>");
                    sb.AppendLine("</div>");
                }
            }

            return new JsonResult(new
            {
                html = sb.ToString(),
                css = "",
                gjsComponents = "",
                gjsStyles = ""
            });
        }

        private static string GetMetadataValue(string? metadata, string key)
        {
            if (string.IsNullOrEmpty(metadata)) return "";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(metadata);
                if (doc.RootElement.TryGetProperty(key, out var prop))
                {
                    return prop.GetString() ?? "";
                }
            }
            catch { }
            return "";
        }
    }
}
