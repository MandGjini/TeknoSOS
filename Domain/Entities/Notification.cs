using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public partial class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string RecipientId { get; set; } = string.Empty;

        public int ServiceRequestId { get; set; }

        public NotificationType Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public bool IsPushSent { get; set; }

        public bool IsEmailSent { get; set; }

        // Navigation properties
        public ApplicationUser Recipient { get; set; } = null!;

        public ServiceRequest ServiceRequest { get; set; } = null!;
    }
}
