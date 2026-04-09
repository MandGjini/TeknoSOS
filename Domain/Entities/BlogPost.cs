using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        [MaxLength(500)]
        public string? Excerpt { get; set; }

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CategoryIcon { get; set; } = "bi-tag";

        [MaxLength(100)]
        public string Author { get; set; } = "TeknoSOS";

        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string ReadTime { get; set; } = "3 min";

        [MaxLength(500)]
        public string ImageIcon { get; set; } = "bi-file-text";

        /// <summary>
        /// Comma-separated tags
        /// </summary>
        [MaxLength(1000)]
        public string? Tags { get; set; }

        /// <summary>
        /// Full HTML content of the article
        /// </summary>
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedDate { get; set; }
    }
}
