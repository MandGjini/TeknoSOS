using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class TechnicianInterest
    {
        public int Id { get; set; }

        public string TechnicianId { get; set; } = string.Empty;

        public int ServiceRequestId { get; set; }

        public InterestStatus Status { get; set; }

        public string? PreventiveOffer { get; set; }

        public decimal? EstimatedCost { get; set; }

        public int? EstimatedTimeInHours { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ResponseDate { get; set; }

        // Navigation properties
        public ApplicationUser Technician { get; set; } = null!;

        public ServiceRequest ServiceRequest { get; set; } = null!;
    }
}
