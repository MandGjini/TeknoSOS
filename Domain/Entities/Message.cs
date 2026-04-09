using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string ReceiverId { get; set; } = string.Empty;

        public int? ServiceRequestId { get; set; }

        [Required]
        [StringLength(4000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Type of message: text, image, document
        /// </summary>
        [Required]
        [StringLength(20)]
        [RegularExpression("^(text|image|document)$", ErrorMessage = "MessageType must be text, image, or document")]
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// URL to attachment file (photo or document)
        /// </summary>
        [StringLength(500)]
        public string? AttachmentUrl { get; set; }

        /// <summary>
        /// Original filename of the attachment
        /// </summary>
        [StringLength(255)]
        public string? AttachmentFileName { get; set; }

        public MessageStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public bool IsArchived { get; set; }

        // Navigation properties
        public ApplicationUser Sender { get; set; } = null!;

        public ApplicationUser Receiver { get; set; } = null!;

        public ServiceRequest? ServiceRequest { get; set; }
    }
}
