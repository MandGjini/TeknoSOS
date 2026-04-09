namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Portfolio item showcasing completed work by a technician
    /// </summary>
    public class TechnicianPortfolio
    {
        public int Id { get; set; }

        public string TechnicianId { get; set; } = string.Empty;

        /// <summary>Job title/description</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Detailed description of the work done</summary>
        public string? Description { get; set; }

        /// <summary>Category of work (maps to ServiceCategory)</summary>
        public string? Category { get; set; }

        /// <summary>Before photo URL</summary>
        public string? BeforeImageUrl { get; set; }

        /// <summary>After photo URL</summary>
        public string? AfterImageUrl { get; set; }

        /// <summary>Additional image URLs (JSON array)</summary>
        public string? AdditionalImagesJson { get; set; }

        /// <summary>Client testimonial</summary>
        public string? ClientTestimonial { get; set; }

        /// <summary>Date the work was completed</summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>Location of the job</summary>
        public string? Location { get; set; }

        /// <summary>Whether this is featured/highlighted</summary>
        public bool IsFeatured { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser Technician { get; set; } = null!;
    }
}
