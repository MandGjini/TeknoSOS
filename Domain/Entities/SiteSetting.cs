using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Stores site-wide settings in the database (replacing site_settings.json).
    /// Key-value pairs for all configurable settings.
    /// </summary>
    public class SiteSetting
    {
        public int Id { get; set; }

        /// <summary>
        /// Setting group (e.g., "branding", "contact", "seo", "social", "languages", "homepage", "general")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string GroupKey { get; set; } = string.Empty;

        /// <summary>
        /// Setting key (e.g., "SiteName", "PrimaryColor", "LogoUrl", "ContactEmail")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>
        /// Setting value
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Display label for the admin UI
        /// </summary>
        [MaxLength(200)]
        public string? DisplayLabel { get; set; }

        /// <summary>
        /// Input type for admin form: "text", "color", "textarea", "url", "email", "select", "toggle"
        /// </summary>
        [MaxLength(50)]
        public string InputType { get; set; } = "text";

        /// <summary>
        /// Comma-separated options for select inputs
        /// </summary>
        public string? SelectOptions { get; set; }

        public int SortOrder { get; set; }

        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    }
}
