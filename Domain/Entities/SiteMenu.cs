using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Stores navigation menu items, editable from admin panel.
    /// </summary>
    public class SiteMenu
    {
        public int Id { get; set; }

        /// <summary>
        /// Menu location: "main", "footer_nav", "footer_legal", "admin_sidebar"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string MenuLocation { get; set; } = "main";

        /// <summary>
        /// Display text (localization key or direct text)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// URL or route (e.g., "/About", "/FAQ", "/Admin")
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional icon class (e.g., "bi-info-circle")
        /// </summary>
        [MaxLength(100)]
        public string? IconClass { get; set; }

        /// <summary>
        /// CSS class for custom styling
        /// </summary>
        [MaxLength(200)]
        public string? CssClass { get; set; }

        /// <summary>
        /// Open in new tab
        /// </summary>
        public bool OpenInNewTab { get; set; }

        /// <summary>
        /// Required role to see this menu item (null = visible to all)
        /// </summary>
        [MaxLength(50)]
        public string? RequiredRole { get; set; }

        /// <summary>
        /// Whether user must be authenticated to see this item
        /// </summary>
        public bool RequiresAuth { get; set; }

        /// <summary>
        /// Only show to unauthenticated users
        /// </summary>
        public bool UnauthenticatedOnly { get; set; }

        /// <summary>
        /// Parent menu item ID for nested menus
        /// </summary>
        public int? ParentId { get; set; }

        public SiteMenu? Parent { get; set; }
        public ICollection<SiteMenu> Children { get; set; } = new List<SiteMenu>();

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
