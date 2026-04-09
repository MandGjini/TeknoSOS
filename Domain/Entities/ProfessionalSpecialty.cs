using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class ProfessionalSpecialty
    {
        public int Id { get; set; }
        
        public string ProfessionalId { get; set; } = string.Empty;
        
        public ServiceCategory Category { get; set; }
        
        public decimal? HourlyRate { get; set; }
        
        public int? YearsOfExperience { get; set; }
        
        public bool IsVerified { get; set; }

        // Navigation properties
        public ApplicationUser Professional { get; set; } = null!;
    }
}
