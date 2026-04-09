using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Data;

namespace TeknoSOS.WebApp.Services
{
    public interface ISitemapService
    {
        Task<string> BuildSitemapXmlAsync(string baseUrl);
    }

    public class SitemapService : ISitemapService
    {
        private readonly ApplicationDbContext _db;

        public SitemapService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> BuildSitemapXmlAsync(string baseUrl)
        {
            baseUrl = NormalizeBaseUrl(baseUrl);

            var staticUrls = new List<(string Path, string ChangeFreq, string Priority)>
            {
                ("/", "daily", "1.0"),
                ("/DefekteElektrike", "weekly", "0.95"),
                ("/KarikuesEV", "weekly", "0.95"),
                ("/HowItWorks", "monthly", "0.9"),
                ("/Technicians", "daily", "0.9"),
                ("/TechniciansMap", "daily", "0.85"),
                ("/Businesses", "daily", "0.85"),
                ("/BusinessMap", "daily", "0.8"),
                ("/About", "monthly", "0.7"),
                ("/Contact", "monthly", "0.7"),
                ("/FAQ", "monthly", "0.7"),
                ("/Blog/Index", "daily", "0.9"),
                ("/Identity/Account/Register", "monthly", "0.8"),
                ("/Identity/Account/Login", "monthly", "0.6"),
                ("/Terms", "yearly", "0.3"),
                ("/Privacy", "yearly", "0.3"),
                ("/CookiePolicy", "yearly", "0.2")
            };

            var blogPosts = await _db.BlogPosts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedDate)
                .Select(p => new
                {
                    p.Slug,
                    p.PublishedDate,
                    p.ModifiedDate
                })
                .ToListAsync();

            var settings = new XmlWriterSettings
            {
                Async = false,
                Indent = true,
                Encoding = new UTF8Encoding(false)
            };

            var sb = new StringBuilder();
            using var writer = XmlWriter.Create(sb, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            foreach (var item in staticUrls)
            {
                WriteUrlNode(
                    writer,
                    BuildAbsoluteUrl(baseUrl, item.Path),
                    DateTime.UtcNow,
                    item.ChangeFreq,
                    item.Priority);
            }

            foreach (var post in blogPosts)
            {
                var lastMod = post.ModifiedDate ?? post.PublishedDate;
                WriteUrlNode(
                    writer,
                    BuildAbsoluteUrl(baseUrl, $"/Blog/{post.Slug}"),
                    lastMod,
                    "weekly",
                    "0.8");
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();

            return sb.ToString();
        }

        private static void WriteUrlNode(XmlWriter writer, string loc, DateTime lastMod, string changeFreq, string priority)
        {
            writer.WriteStartElement("url");

            writer.WriteElementString("loc", loc);
            writer.WriteElementString("lastmod", lastMod.ToUniversalTime().ToString("yyyy-MM-dd"));
            writer.WriteElementString("changefreq", changeFreq);
            writer.WriteElementString("priority", priority);

            writer.WriteEndElement();
        }

        private static string NormalizeBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return "https://teknosos.app";

            return baseUrl.TrimEnd('/');
        }

        private static string BuildAbsoluteUrl(string baseUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                return $"{baseUrl}/";

            return $"{baseUrl}{(path.StartsWith('/') ? path : "/" + path)}";
        }
    }
}