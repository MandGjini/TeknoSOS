using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public interface ISiteContentService
    {
        Task<string> GetContentAsync(string pageKey, string sectionKey, string? language = null);
        Task<string?> GetTitleAsync(string pageKey, string sectionKey, string? language = null);
        Task<SiteContent?> GetBlockAsync(string pageKey, string sectionKey, string? language = null);
        Task<SiteContent?> GetContentByIdAsync(int id);
        Task<List<SiteContent>> GetPageContentAsync(string pageKey, string? language = null);
        Task<List<SiteContent>> GetPageContentByTypeAsync(string pageKey, string contentType, string? language = null);
        Task<List<string>> GetDistinctPagesAsync();
        Task SaveContentAsync(SiteContent content, string? modifiedBy = null);
        Task SaveBulkContentAsync(IEnumerable<SiteContent> contents, string? modifiedBy = null);
        Task DeleteContentAsync(int id);
        Task<bool> HasContentAsync(string pageKey, string? language = null);
        string CurrentLanguage { get; }
    }

    public class SiteContentService : ISiteContentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SiteContentService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public string CurrentLanguage
        {
            get
            {
                var cookie = _httpContextAccessor.HttpContext?.Request.Cookies["teknosos_lang"];
                return string.IsNullOrEmpty(cookie) ? "sq" : cookie;
            }
        }

        public async Task<string> GetContentAsync(string pageKey, string sectionKey, string? language = null)
        {
            var lang = language ?? CurrentLanguage;
            var content = await _db.SiteContents
                .Where(c => c.PageKey == pageKey && c.SectionKey == sectionKey && c.Language == lang && c.IsActive)
                .Select(c => c.Content)
                .FirstOrDefaultAsync();

            if (content == null && lang != "sq")
            {
                content = await _db.SiteContents
                    .Where(c => c.PageKey == pageKey && c.SectionKey == sectionKey && c.Language == "sq" && c.IsActive)
                    .Select(c => c.Content)
                    .FirstOrDefaultAsync();
            }

            return content ?? string.Empty;
        }

        public async Task<string?> GetTitleAsync(string pageKey, string sectionKey, string? language = null)
        {
            var lang = language ?? CurrentLanguage;
            var title = await _db.SiteContents
                .Where(c => c.PageKey == pageKey && c.SectionKey == sectionKey && c.Language == lang && c.IsActive)
                .Select(c => c.Title)
                .FirstOrDefaultAsync();

            if (title == null && lang != "sq")
            {
                title = await _db.SiteContents
                    .Where(c => c.PageKey == pageKey && c.SectionKey == sectionKey && c.Language == "sq" && c.IsActive)
                    .Select(c => c.Title)
                    .FirstOrDefaultAsync();
            }

            return title;
        }

        public async Task<SiteContent?> GetBlockAsync(string pageKey, string sectionKey, string? language = null)
        {
            var lang = language ?? CurrentLanguage;
            var block = await _db.SiteContents
                .Where(c => c.PageKey == pageKey && c.SectionKey == sectionKey && c.Language == lang && c.IsActive)
                .FirstOrDefaultAsync();

            if (block == null && lang != "sq")
            {
                block = await _db.SiteContents
                    .Where(c => c.PageKey == pageKey && c.SectionKey == sectionKey && c.Language == "sq" && c.IsActive)
                    .FirstOrDefaultAsync();
            }

            return block;
        }

        public async Task<List<SiteContent>> GetPageContentAsync(string pageKey, string? language = null)
        {
            var lang = language ?? CurrentLanguage;
            var content = await _db.SiteContents
                .Where(c => c.PageKey == pageKey && c.Language == lang && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            if (!content.Any() && lang != "sq")
            {
                content = await _db.SiteContents
                    .Where(c => c.PageKey == pageKey && c.Language == "sq" && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();
            }

            return content;
        }

        public async Task<List<SiteContent>> GetPageContentByTypeAsync(string pageKey, string contentType, string? language = null)
        {
            var lang = language ?? CurrentLanguage;
            var content = await _db.SiteContents
                .Where(c => c.PageKey == pageKey && c.ContentType == contentType && c.Language == lang && c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            if (!content.Any() && lang != "sq")
            {
                content = await _db.SiteContents
                    .Where(c => c.PageKey == pageKey && c.ContentType == contentType && c.Language == "sq" && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();
            }

            return content;
        }

        public async Task<SiteContent?> GetContentByIdAsync(int id)
        {
            return await _db.SiteContents.FindAsync(id);
        }

        public async Task<List<string>> GetDistinctPagesAsync()
        {
            return await _db.SiteContents
                .Select(c => c.PageKey)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task SaveContentAsync(SiteContent content, string? modifiedBy = null)
        {
            content.LastModifiedDate = DateTime.UtcNow;
            content.LastModifiedBy = modifiedBy;

            if (content.Id > 0)
            {
                // Update by ID (from admin panel edit)
                var existing = await _db.SiteContents.FindAsync(content.Id);
                if (existing != null)
                {
                    existing.PageKey = content.PageKey;
                    existing.SectionKey = content.SectionKey;
                    existing.Language = content.Language;
                    existing.Title = content.Title;
                    existing.Content = content.Content;
                    existing.ImageUrl = content.ImageUrl;
                    existing.IconClass = content.IconClass;
                    existing.SortOrder = content.SortOrder;
                    existing.IsActive = content.IsActive;
                    existing.ContentType = content.ContentType;
                    existing.Metadata = content.Metadata;
                    existing.LastModifiedDate = DateTime.UtcNow;
                    existing.LastModifiedBy = modifiedBy;
                }
            }
            else
            {
                // Insert or update by composite key (from seeder)
                var existing = await _db.SiteContents
                    .FirstOrDefaultAsync(c => c.PageKey == content.PageKey && c.SectionKey == content.SectionKey && c.Language == content.Language);

                if (existing != null)
                {
                    existing.Title = content.Title;
                    existing.Content = content.Content;
                    existing.ImageUrl = content.ImageUrl;
                    existing.IconClass = content.IconClass;
                    existing.SortOrder = content.SortOrder;
                    existing.IsActive = content.IsActive;
                    existing.ContentType = content.ContentType;
                    existing.Metadata = content.Metadata;
                    existing.LastModifiedDate = DateTime.UtcNow;
                    existing.LastModifiedBy = modifiedBy;
                }
                else
                {
                    content.CreatedDate = DateTime.UtcNow;
                    _db.SiteContents.Add(content);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task SaveBulkContentAsync(IEnumerable<SiteContent> contents, string? modifiedBy = null)
        {
            var contentsList = contents.ToList();
            if (!contentsList.Any()) return;

            // Build composite keys for lookup
            var lookupKeys = contentsList
                .Select(c => new { c.PageKey, c.SectionKey, c.Language })
                .ToList();

            // Fetch all existing records in ONE query instead of N queries
            var existingRecords = await _db.SiteContents
                .Where(c => lookupKeys.Any(k => 
                    k.PageKey == c.PageKey && 
                    k.SectionKey == c.SectionKey && 
                    k.Language == c.Language))
                .ToListAsync();

            // Create lookup dictionary for O(1) access
            var existingLookup = existingRecords.ToDictionary(
                c => (c.PageKey, c.SectionKey, c.Language), 
                c => c);

            foreach (var content in contentsList)
            {
                content.LastModifiedDate = DateTime.UtcNow;
                content.LastModifiedBy = modifiedBy;

                var key = (content.PageKey, content.SectionKey, content.Language);
                
                if (existingLookup.TryGetValue(key, out var existing))
                {
                    // Update existing record
                    existing.Title = content.Title;
                    existing.Content = content.Content;
                    existing.ImageUrl = content.ImageUrl;
                    existing.IconClass = content.IconClass;
                    existing.SortOrder = content.SortOrder;
                    existing.IsActive = content.IsActive;
                    existing.ContentType = content.ContentType;
                    existing.Metadata = content.Metadata;
                    existing.LastModifiedDate = DateTime.UtcNow;
                    existing.LastModifiedBy = modifiedBy;
                }
                else
                {
                    // Insert new record
                    content.CreatedDate = DateTime.UtcNow;
                    _db.SiteContents.Add(content);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteContentAsync(int id)
        {
            var content = await _db.SiteContents.FindAsync(id);
            if (content != null)
            {
                _db.SiteContents.Remove(content);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> HasContentAsync(string pageKey, string? language = null)
        {
            var lang = language ?? "sq";
            return await _db.SiteContents.AnyAsync(c => c.PageKey == pageKey && c.Language == lang);
        }
    }
}
