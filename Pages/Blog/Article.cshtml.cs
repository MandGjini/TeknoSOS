using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Services;

namespace TeknoSOS.WebApp.Pages.Blog
{
    public class ArticleModel : PageModel
    {
        private readonly IBlogService _blogService;

        public ArticleModel(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public BlogArticle? Article { get; set; }
        public List<BlogArticle> RelatedArticles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return RedirectToPage("/Blog/Index");

            var dbPost = await _blogService.GetBySlugAsync(slug);
            if (dbPost != null)
            {
                Article = MapToArticle(dbPost);

                var allPosts = await _blogService.GetAllActiveAsync();
                RelatedArticles = allPosts
                    .Where(a => a.Slug != slug)
                    .OrderByDescending(a => a.Category == dbPost.Category)
                    .ThenByDescending(a => a.PublishedDate)
                    .Take(3)
                    .Select(MapToArticle)
                    .ToList();
            }
            else
            {
                // Fallback to static data
                Article = BlogData.GetBySlug(slug);
                if (Article == null)
                    return NotFound();

                RelatedArticles = BlogData.GetAllArticles()
                    .Where(a => a.Slug != slug)
                    .OrderByDescending(a => a.Category == Article.Category)
                    .ThenByDescending(a => a.PublishedDate)
                    .Take(3)
                    .ToList();
            }

            return Page();
        }

        private static BlogArticle MapToArticle(Domain.Entities.BlogPost p) => new()
        {
            Slug = p.Slug,
            Title = p.Title,
            MetaDescription = p.MetaDescription ?? "",
            Excerpt = p.Excerpt ?? "",
            Category = p.Category,
            CategoryIcon = p.CategoryIcon,
            Author = p.Author,
            PublishedDate = p.PublishedDate,
            ModifiedDate = p.ModifiedDate,
            ReadTime = p.ReadTime,
            ImageIcon = p.ImageIcon,
            Tags = (p.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            Content = p.Content
        };
    }
}
