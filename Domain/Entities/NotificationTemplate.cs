using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Stores customizable email and SMS notification templates.
    /// Admins can edit these templates from the admin panel.
    /// </summary>
    public class NotificationTemplate
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique key to identify the template (e.g., "technician_welcome", "technician_activated", "citizen_welcome")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TemplateKey { get; set; } = string.Empty;

        /// <summary>
        /// Display name for admin UI
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Description explaining when this template is used
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Template type: Email or SMS
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string TemplateType { get; set; } = "Email"; // "Email" or "SMS"

        /// <summary>
        /// Email subject line (for Email templates only)
        /// </summary>
        [MaxLength(200)]
        public string? Subject { get; set; }

        /// <summary>
        /// Template content - HTML for emails, plain text for SMS.
        /// Supports placeholders: {{FirstName}}, {{LastName}}, {{Email}}, {{City}}, etc.
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Is this template enabled?
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Category for organization (e.g., "Registration", "Verification", "Notifications")
        /// </summary>
        [MaxLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Sort order for display
        /// </summary>
        public int SortOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
    }
}
