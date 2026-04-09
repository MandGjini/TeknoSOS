using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Stores editable page content for the CMS system.
    /// Each record represents a content block on a specific page in a specific language.
    /// </summary>
    public class SiteContent
    {
        public int Id { get; set; }

        /// <summary>
        /// Identifies the page (e.g., "home", "about", "faq", "terms", "privacy", "contact", "howitworks", "cookie", "disclaimer", "dataprocessing")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PageKey { get; set; } = string.Empty;

        /// <summary>
        /// Identifies the section within the page (e.g., "hero_title", "hero_subtitle", "section_1_title", "section_1_body", "faq_1_q", "faq_1_a")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string SectionKey { get; set; } = string.Empty;

        /// <summary>
        /// Language code: "sq", "en", "it", "de", "fr"
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Language { get; set; } = "sq";

        /// <summary>
        /// The display title of this content block (optional, used for headings)
        /// </summary>
        [MaxLength(500)]
        public string? Title { get; set; }

        /// <summary>
        /// The main content (HTML allowed for rich text)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Optional image URL associated with this content block
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Optional icon class (e.g., "bi-shield-check")
        /// </summary>
        [MaxLength(100)]
        public string? IconClass { get; set; }

        /// <summary>
        /// Sort order within the page and language
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Whether this content block is visible
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Content type: "text", "html", "faq", "testimonial", "feature", "step", "stat"
        /// </summary>
        [MaxLength(50)]
        public string ContentType { get; set; } = "text";

        /// <summary>
        /// Optional metadata stored as JSON (for extra fields)
        /// </summary>
        public string? Metadata { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        public string? LastModifiedBy { get; set; }
    }
}
