using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Domain.Enums;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique tracking code in format DEF-000001
        /// </summary>
        [Required]
        [StringLength(20)]
        public string UniqueCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string CitizenId { get; set; } = string.Empty;
        
        [StringLength(450)]
        public string? ProfessionalId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;
        
        public ServiceCategory Category { get; set; }
        
        public ServiceRequestStatus Status { get; set; }
        
        public ServiceRequestPriority Priority { get; set; }

        public CasePriority CasePriority { get; set; } = CasePriority.Medium;

        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [StringLength(50)]
        [Phone]
        public string? ClientContactNumber { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? ScheduledDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(500)]
        public string? Location { get; set; }

        [Range(-90, 90)]
        public decimal? ClientLatitude { get; set; }

        [Range(-180, 180)]
        public decimal? ClientLongitude { get; set; }

        [Range(0, 100000)]
        public decimal? NearestTechnicianDistance { get; set; }
        
        [Range(0, 1000000)]
        public decimal? EstimatedCost { get; set; }
        
        [Range(0, 1000000)]
        public decimal? FinalCost { get; set; }
        
        public bool IsUrgent => Priority == ServiceRequestPriority.Emergency || Priority == ServiceRequestPriority.High || CasePriority == CasePriority.Emergency;

        // Navigation properties
        public ApplicationUser Citizen { get; set; } = null!;
        
        public ApplicationUser? Professional { get; set; }
        
        public Review? Review { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<Message> Messages { get; set; } = new List<Message>();

        public ICollection<TechnicianInterest> TechnicianInterests { get; set; } = new List<TechnicianInterest>();
    }
}
