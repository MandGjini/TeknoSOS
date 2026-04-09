using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Services
{
    public interface IBannerService
    {
        Task<List<Banner>> GetActiveBannersAsync(string position, string? pageKey = null);
        Task<List<Banner>> GetAllBannersForPageAsync(string pageKey);
        Task IncrementViewAsync(int bannerId);
        Task IncrementClickAsync(int bannerId);
    }

    public class BannerService : IBannerService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<BannerService> _logger;

        public BannerService(ApplicationDbContext db, ILogger<BannerService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Get active banners for a specific position and page
        /// </summary>
        public async Task<List<Banner>> GetActiveBannersAsync(string position, string? pageKey = null)
        {
            var now = DateTime.UtcNow;

            var query = _db.Banners
                .Where(b => b.IsActive)
                .Where(b => b.Position == position)
                .Where(b => !b.StartDate.HasValue || b.StartDate <= now)
                .Where(b => !b.EndDate.HasValue || b.EndDate >= now);

            var banners = await query
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            // Filter by target pages if pageKey is provided
            if (!string.IsNullOrEmpty(pageKey))
            {
                banners = banners
                    .Where(b => MatchesTargetPage(b.TargetPages, pageKey))
                    .ToList();
            }

            return banners;
        }

        /// <summary>
        /// Get all active banners for a specific page (all positions)
        /// </summary>
        public async Task<List<Banner>> GetAllBannersForPageAsync(string pageKey)
        {
            var now = DateTime.UtcNow;

            var banners = await _db.Banners
                .Where(b => b.IsActive)
                .Where(b => !b.StartDate.HasValue || b.StartDate <= now)
                .Where(b => !b.EndDate.HasValue || b.EndDate >= now)
                .OrderBy(b => b.Position)
                .ThenBy(b => b.DisplayOrder)
                .ToListAsync();

            // Filter by target pages
            return banners
                .Where(b => MatchesTargetPage(b.TargetPages, pageKey))
                .ToList();
        }

        private static bool MatchesTargetPage(string? targetPages, string pageKey)
        {
            if (string.IsNullOrWhiteSpace(targetPages))
            {
                return true;
            }

            var candidates = BuildPageCandidates(pageKey);
            var targets = targetPages
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeToken)
                .Where(t => !string.IsNullOrEmpty(t));

            foreach (var target in targets)
            {
                if (target == "*")
                {
                    return true;
                }

                if (target.EndsWith("/*", StringComparison.Ordinal))
                {
                    var prefix = target[..^2];
                    if (candidates.Any(c => c == prefix || c.StartsWith(prefix + "/", StringComparison.Ordinal)))
                    {
                        return true;
                    }

                    continue;
                }

                if (candidates.Contains(target))
                {
                    return true;
                }
            }

            return false;
        }

        private static HashSet<string> BuildPageCandidates(string? rawPageKey)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            var normalized = NormalizeToken(rawPageKey);

            if (string.IsNullOrEmpty(normalized))
            {
                set.Add("home");
                set.Add("index");
                return set;
            }

            set.Add(normalized);

            if (normalized == "home" || normalized == "index")
            {
                set.Add("home");
                set.Add("index");
            }

            if (normalized.StartsWith("blog/", StringComparison.Ordinal))
            {
                set.Add("blog");
            }

            if (normalized.StartsWith("admin/", StringComparison.Ordinal))
            {
                set.Add("admin");
            }

            return set;
        }

        private static string NormalizeToken(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var token = Uri.UnescapeDataString(value).Trim().ToLowerInvariant();

            if (token.StartsWith("~/", StringComparison.Ordinal))
            {
                token = token[2..];
            }

            token = token.Trim('/');

            if (token == string.Empty)
            {
                return "home";
            }

            return token;
        }

        /// <summary>
        /// Increment view count for a banner using atomic SQL update
        /// </summary>
        public async Task IncrementViewAsync(int bannerId)
        {
            try
            {
                // Use atomic SQL increment to prevent race conditions
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Banners SET ViewCount = ViewCount + 1 WHERE Id = {bannerId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment view count for banner {BannerId}", bannerId);
            }
        }

        /// <summary>
        /// Increment click count for a banner using atomic SQL update
        /// </summary>
        public async Task IncrementClickAsync(int bannerId)
        {
            try
            {
                // Use atomic SQL increment to prevent race conditions
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Banners SET ClickCount = ClickCount + 1 WHERE Id = {bannerId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment click count for banner {BannerId}", bannerId);
            }
        }
    }
}
