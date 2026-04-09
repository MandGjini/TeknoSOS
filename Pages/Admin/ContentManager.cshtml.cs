using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ContentManagerModel : PageModel
    {
        private readonly ISiteContentService _cms;

        public ContentManagerModel(ISiteContentService cms)
        {
            _cms = cms;
        }

        [BindProperty(SupportsGet = true)]
        public string? SelectedPage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedLang { get; set; }

        public List<SiteContent> ContentItems { get; set; } = new();
        public List<string> AvailablePages { get; set; } = new();
        public string? SuccessMsg { get; set; }

        // For edit form
        [BindProperty]
        public int EditId { get; set; }
        [BindProperty]
        public string? EditPageKey { get; set; }
        [BindProperty]
        public string? EditSectionKey { get; set; }
        [BindProperty]
        public string? EditLanguage { get; set; }
        [BindProperty]
        public string? EditTitle { get; set; }
        [BindProperty]
        public string? EditContent { get; set; }
        [BindProperty]
        public string? EditImageUrl { get; set; }
        [BindProperty]
        public string? EditIconClass { get; set; }
        [BindProperty]
        public int EditSortOrder { get; set; }
        [BindProperty]
        public bool EditIsActive { get; set; } = true;
        [BindProperty]
        public string? EditContentType { get; set; }
        [BindProperty]
        public string? EditMetadata { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMsg = TempData["Success"]?.ToString();
            SelectedLang ??= "sq";
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            // Get all distinct page keys
            AvailablePages = await _cms.GetDistinctPagesAsync();

            if (!string.IsNullOrEmpty(SelectedPage))
            {
                ContentItems = await _cms.GetPageContentAsync(SelectedPage, SelectedLang ?? "sq");
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var item = new SiteContent
            {
                Id = EditId,
                PageKey = EditPageKey ?? "",
                SectionKey = EditSectionKey ?? "",
                Language = EditLanguage ?? "sq",
                Title = EditTitle ?? "",
                Content = EditContent ?? "",
                ImageUrl = EditImageUrl,
                IconClass = EditIconClass,
                SortOrder = EditSortOrder,
                IsActive = EditIsActive,
                ContentType = EditContentType ?? "text",
                Metadata = EditMetadata
            };

            await _cms.SaveContentAsync(item);
            TempData["Success"] = EditId > 0 ? "Permbajtja u perditesua." : "Permbajtja u shtua me sukses.";
            return RedirectToPage(new { selectedPage = item.PageKey, selectedLang = item.Language });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string? returnPage, string? returnLang)
        {
            await _cms.DeleteContentAsync(id);
            TempData["Success"] = "Permbajtja u fshi.";
            return RedirectToPage(new { selectedPage = returnPage, selectedLang = returnLang });
        }

        public async Task<IActionResult> OnPostDuplicateAsync(int id, string targetLang, string? returnPage, string? returnLang)
        {
            var source = await _cms.GetContentByIdAsync(id);
            if (source != null)
            {
                var dup = new SiteContent
                {
                    PageKey = source.PageKey,
                    SectionKey = source.SectionKey,
                    Language = targetLang,
                    Title = source.Title,
                    Content = source.Content,
                    ImageUrl = source.ImageUrl,
                    IconClass = source.IconClass,
                    SortOrder = source.SortOrder,
                    IsActive = source.IsActive,
                    ContentType = source.ContentType,
                    Metadata = source.Metadata
                };
                await _cms.SaveContentAsync(dup);
                TempData["Success"] = $"Permbajtja u kopjua ne gjuhen '{targetLang}'.";
            }
            return RedirectToPage(new { selectedPage = returnPage, selectedLang = targetLang });
        }
    }
}
