using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BlogManagerModel : PageModel
    {
        private readonly IBlogService _blogService;

        public BlogManagerModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public List<BlogPost> Posts { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty] public int? EditId { get; set; }
        [BindProperty] public string Slug { get; set; } = string.Empty;
        [BindProperty] public string Title { get; set; } = string.Empty;
        [BindProperty] public string? MetaDescription { get; set; }
        [BindProperty] public string? Excerpt { get; set; }
        [BindProperty] public string Category { get; set; } = string.Empty;
        [BindProperty] public string CategoryIcon { get; set; } = "bi-tag";
        [BindProperty] public string Author { get; set; } = "TeknoSOS";
        [BindProperty] public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        [BindProperty] public string ReadTime { get; set; } = "3 min";
        [BindProperty] public string ImageIcon { get; set; } = "bi-file-text";
        [BindProperty] public string? Tags { get; set; }
        [BindProperty] public string Body { get; set; } = string.Empty;
        [BindProperty] public bool IsActive { get; set; } = true;
        [BindProperty] public int SortOrder { get; set; }

        public async Task OnGetAsync()
        {
            SuccessMessage = TempData["Success"]?.ToString();
            ErrorMessage = TempData["Error"]?.ToString();
            Posts = await _blogService.GetAllAsync() ?? new List<BlogPost>();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Slug))
            {
                TempData["Error"] = "Titulli dhe Slug jane te detyrueshem.";
                return RedirectToPage();
            }

            await _blogService.CreateAsync(new BlogPost
            {
                Slug = Slug.Trim().ToLower(),
                Title = Title,
                MetaDescription = MetaDescription,
                Excerpt = Excerpt,
                Category = Category,
                CategoryIcon = CategoryIcon,
                Author = Author,
                PublishedDate = PublishedDate,
                ReadTime = ReadTime,
                ImageIcon = ImageIcon,
                Tags = Tags,
                Content = Body,
                IsActive = IsActive,
                SortOrder = SortOrder
            });

            TempData["Success"] = $"Artikulli '{Title}' u krijua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!EditId.HasValue)
            {
                TempData["Error"] = "ID e artikullit mungon.";
                return RedirectToPage();
            }

            await _blogService.UpdateAsync(new BlogPost
            {
                Id = EditId.Value,
                Slug = Slug.Trim().ToLower(),
                Title = Title,
                MetaDescription = MetaDescription,
                Excerpt = Excerpt,
                Category = Category,
                CategoryIcon = CategoryIcon,
                Author = Author,
                PublishedDate = PublishedDate,
                ReadTime = ReadTime,
                ImageIcon = ImageIcon,
                Tags = Tags,
                Content = Body,
                IsActive = IsActive,
                SortOrder = SortOrder
            });

            TempData["Success"] = $"Artikulli '{Title}' u perditesua me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var post = await _blogService.GetByIdAsync(id);
            if (post == null)
            {
                TempData["Error"] = "Artikulli nuk u gjet.";
                return RedirectToPage();
            }

            var title = post.Title;
            await _blogService.DeleteAsync(id);

            TempData["Success"] = $"Artikulli '{title}' u fshi me sukses.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            var post = await _blogService.GetByIdAsync(id);
            if (post == null)
            {
                TempData["Error"] = "Artikulli nuk u gjet.";
                return RedirectToPage();
            }

            await _blogService.ToggleActiveAsync(id);
            var updatedPost = await _blogService.GetByIdAsync(id);

            TempData["Success"] = $"Artikulli '{post.Title}' u {(updatedPost?.IsActive == true ? "aktivizua" : "caktivizua")}.";
            return RedirectToPage();
        }
    }
}
