using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public interface IBlogService
    {
        Task<List<BlogPost>> GetAllActiveAsync();
        Task<List<BlogPost>> GetAllAsync();
        Task<BlogPost?> GetBySlugAsync(string slug);
        Task<BlogPost?> GetByIdAsync(int id);
        Task CreateAsync(BlogPost post);
        Task UpdateAsync(BlogPost post);
        Task DeleteAsync(int id);
        Task ToggleActiveAsync(int id);
        Task SeedFromStaticDataAsync();
    }

    public class BlogService : IBlogService
    {
        private readonly ApplicationDbContext _db;

        public BlogService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<BlogPost>> GetAllActiveAsync()
        {
            return await _db.BlogPosts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetAllAsync()
        {
            return await _db.BlogPosts
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            return await _db.BlogPosts
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _db.BlogPosts.FindAsync(id);
        }

        public async Task CreateAsync(BlogPost post)
        {
            post.CreatedDate = DateTime.UtcNow;
            _db.BlogPosts.Add(post);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(BlogPost post)
        {
            var existing = await _db.BlogPosts.FindAsync(post.Id);
            if (existing == null) return;

            existing.Slug = post.Slug;
            existing.Title = post.Title;
            existing.MetaDescription = post.MetaDescription;
            existing.Excerpt = post.Excerpt;
            existing.Category = post.Category;
            existing.CategoryIcon = post.CategoryIcon;
            existing.Author = post.Author;
            existing.PublishedDate = post.PublishedDate;
            existing.ReadTime = post.ReadTime;
            existing.ImageIcon = post.ImageIcon;
            existing.Tags = post.Tags;
            existing.Content = post.Content;
            existing.IsActive = post.IsActive;
            existing.SortOrder = post.SortOrder;
            existing.ModifiedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var post = await _db.BlogPosts.FindAsync(id);
            if (post == null) return;
            _db.BlogPosts.Remove(post);
            await _db.SaveChangesAsync();
        }

        public async Task ToggleActiveAsync(int id)
        {
            var post = await _db.BlogPosts.FindAsync(id);
            if (post == null) return;
            post.IsActive = !post.IsActive;
            post.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task SeedFromStaticDataAsync()
        {
            if (await _db.BlogPosts.AnyAsync()) return;

            // Import from static BlogData
            var staticArticles = Pages.Blog.BlogData.GetAllArticles();
            foreach (var article in staticArticles)
            {
                _db.BlogPosts.Add(new BlogPost
                {
                    Slug = article.Slug,
                    Title = article.Title,
                    MetaDescription = article.MetaDescription,
                    Excerpt = article.Excerpt,
                    Category = article.Category,
                    CategoryIcon = article.CategoryIcon,
                    Author = article.Author,
                    PublishedDate = article.PublishedDate,
                    ReadTime = article.ReadTime,
                    ImageIcon = article.ImageIcon,
                    Tags = string.Join(",", article.Tags),
                    Content = article.Content,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}
