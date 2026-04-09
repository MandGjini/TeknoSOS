using System.ComponentModel.DataAnnotations;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        
        public int ServiceRequestId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string ReviewerId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string RevieweeId { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } // 1-5 stars
        
        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public ServiceRequest ServiceRequest { get; set; } = null!;
        
        public ApplicationUser Reviewer { get; set; } = null!;
        
        public ApplicationUser Reviewee { get; set; } = null!;
    }
}
