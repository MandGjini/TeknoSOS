using System;
using System.Collections.Generic;
using System.Linq;
using TeknoSOS.WebApp.Data;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Data.Seed;

namespace TeknoSOS.WebApp.Data.Seed
{
    public static class BlogSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            var posts = BlogSeedData.GetPosts();
            // Also seed English variant of the first posts if present
            var enPosts = new List<(string Title, string Content, string CallToAction, string ImageUrl, string Category, string Tags)>();
            try
            {
                enPosts = BlogSeedDataEn.GetPosts();
            }
            catch
            {
                // ignore if English seed not present
            }
            var existingSlugs = db.BlogPosts
                .Select(p => p.Slug)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            int i = db.BlogPosts.Count() + 1;
            var toAdd = new List<BlogPost>();

            foreach (var (title, content, cta, imageUrl, category, tags) in posts.Concat(enPosts))
            {
                var slug = $"blog-{i}";
                if (existingSlugs.Contains(slug))
                {
                    i++;
                    continue;
                }

                toAdd.Add(new BlogPost
                {
                    Title = title,
                    Slug = slug,
                    MetaDescription = content.Length > 150 ? content[..150] + "..." : content,
                    Excerpt = content.Length > 120 ? content[..120] + "..." : content,
                    Category = category,
                    CategoryIcon = "bi-card-image",
                    Author = "TeknoSOS",
                    PublishedDate = DateTime.UtcNow.AddMinutes(-i),
                    ReadTime = "4 min",
                    ImageIcon = "bi-card-image",
                    Tags = tags,
                    Content = $"<img src='{imageUrl}' alt='{title}' style='width:100%;height:auto;margin-bottom:16px;' />{content}\n\n<p><strong>{cta}</strong></p>",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow.AddMinutes(-i)
                });
                i++;
            }

            // Massive SEO-oriented long-form content: 70 SQ + 70 EN.
            foreach (var generated in BlogMassContentGenerator.GenerateSeoPosts())
            {
                if (existingSlugs.Contains(generated.Slug))
                {
                    continue;
                }

                toAdd.Add(generated);
                existingSlugs.Add(generated.Slug);
            }

            if (toAdd.Any())
            {
                db.BlogPosts.AddRange(toAdd);
                db.SaveChanges();
            }
        }
    }
}
