using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Represents a banner/advertisement that can be displayed on platform pages.
    /// </summary>
    public class Banner
    {
        public int Id { get; set; }

        /// <summary>
        /// Internal name for the banner (for admin reference)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Position where the banner should display:
        /// "header" - Top of page below navbar
        /// "sidebar" - Right sidebar
        /// "footer" - Above footer
        /// "inline" - Between content sections
        /// "popup" - Modal popup (shown once per session)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Position { get; set; } = "header";

        /// <summary>
        /// Image URL for the banner
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Alternative image URL for mobile devices
        /// </summary>
        [MaxLength(500)]
        public string? MobileImageUrl { get; set; }

        /// <summary>
        /// HTML content (alternative to image)
        /// </summary>
        public string? HtmlContent { get; set; }

        /// <summary>
        /// Link URL when banner is clicked
        /// </summary>
        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Link target: "_self", "_blank"
        /// </summary>
        [MaxLength(20)]
        public string LinkTarget { get; set; } = "_blank";

        /// <summary>
        /// Alt text for accessibility
        /// </summary>
        [MaxLength(300)]
        public string? AltText { get; set; }

        /// <summary>
        /// Pages where this banner should appear (comma-separated, e.g., "home,about,technicians" or "*" for all)
        /// </summary>
        [MaxLength(500)]
        public string TargetPages { get; set; } = "*";

        /// <summary>
        /// Display order (lower = first)
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Whether the banner is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Start date for scheduled display (null = immediate)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for scheduled display (null = no end)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Click count for analytics
        /// </summary>
        public int ClickCount { get; set; } = 0;

        /// <summary>
        /// View/impression count for analytics
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Background color (hex, e.g., "#ffffff")
        /// </summary>
        [MaxLength(20)]
        public string? BackgroundColor { get; set; }

        /// <summary>
        /// Whether to show close button
        /// </summary>
        public bool ShowCloseButton { get; set; } = false;

        /// <summary>
        /// Created timestamp
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
