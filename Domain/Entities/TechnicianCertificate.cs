namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Professional certificate/qualification for a technician
    /// </summary>
    public class TechnicianCertificate
    {
        public int Id { get; set; }

        public string TechnicianId { get; set; } = string.Empty;

        /// <summary>Certificate title (e.g., "Licensed Electrician")</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Issuing authority (e.g., "National Electricians Board")</summary>
        public string? IssuedBy { get; set; }

        /// <summary>Certificate number/ID</summary>
        public string? CertificateNumber { get; set; }

        /// <summary>Date certificate was issued</summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>Expiry date (null = no expiry)</summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>URL to uploaded certificate document/image</summary>
        public string? DocumentUrl { get; set; }

        /// <summary>Verified by admin</summary>
        public bool IsVerified { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser Technician { get; set; } = null!;
    }
}
