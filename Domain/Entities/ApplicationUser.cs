using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Unique display username (e.g. "admin", "tekniku_edi"). 
        /// Used in chat, reports, profile, and activity history. Only admin can change it.
        /// </summary>
        public string? DisplayUsername { get; set; }
        
        public UserRole Role { get; set; }
        
        public DateTime RegistrationDate { get; set; }
        
        public bool IsActive { get; set; }
        
        public string? ProfileImageUrl { get; set; }
        
        public string? Address { get; set; }
        
        public string? City { get; set; }
        
        public string? PostalCode { get; set; }

        // New properties
        public Language PreferredLanguage { get; set; } = Language.English;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public bool NotificationsEnabled { get; set; } = true;

        public bool EmailNotificationsEnabled { get; set; } = true;

        public bool PushNotificationsEnabled { get; set; } = true;

        public DateTime? LastLoginDate { get; set; }

        // ======== Technician Profile Fields ========
        /// <summary>Bio/description for technician profile</summary>
        public string? Bio { get; set; }

        /// <summary>Total years of professional experience</summary>
        public int? YearsOfExperience { get; set; }

        /// <summary>Whether technician is currently available for new jobs</summary>
        public bool IsAvailableForWork { get; set; } = true;

        /// <summary>Technician's company/business name</summary>
        public string? CompanyName { get; set; }

        /// <summary>Business license number</summary>
        public string? LicenseNumber { get; set; }

        /// <summary>Comma-separated spoken languages</summary>
        public string? SpokenLanguages { get; set; }

        /// <summary>Working hours description (e.g., "Mon-Fri 08:00-17:00")</summary>
        public string? WorkingHours { get; set; }

        /// <summary>Service radius in kilometers</summary>
        public int? ServiceRadiusKm { get; set; }

        /// <summary>Total completed jobs count</summary>
        public int CompletedJobsCount { get; set; }

        /// <summary>Profile verified by admin</summary>
        public bool IsProfileVerified { get; set; }

        /// <summary>Whether technician has uploaded at least one certificate</summary>
        public bool HasUploadedCertificates { get; set; }

        /// <summary>Whether technician has uploaded a profile photo</summary>
        public bool HasUploadedProfilePhoto { get; set; }

        /// <summary>Date when technician was verified by admin</summary>
        public DateTime? VerificationDate { get; set; }

        /// <summary>Admin notes about verification</summary>
        public string? VerificationNotes { get; set; }

        // Navigation properties
        public ICollection<ServiceRequest> CreatedServiceRequests { get; set; } = new List<ServiceRequest>();
        
        public ICollection<ServiceRequest> AssignedServiceRequests { get; set; } = new List<ServiceRequest>();
        
        public ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();
        
        public ICollection<Review> WrittenReviews { get; set; } = new List<Review>();
        
        public ICollection<ProfessionalSpecialty> Specialties { get; set; } = new List<ProfessionalSpecialty>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<Message> SentMessages { get; set; } = new List<Message>();

        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        public ICollection<TechnicianInterest> Interests { get; set; } = new List<TechnicianInterest>();

        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public ICollection<TechnicianCertificate> Certificates { get; set; } = new List<TechnicianCertificate>();

        public ICollection<TechnicianPortfolio> PortfolioItems { get; set; } = new List<TechnicianPortfolio>();
        public ICollection<TechnicianSubscription> Subscriptions { get; set; } = new List<TechnicianSubscription>();

        // ======== Subscription Fields ========
        /// <summary>When the current paid subscription expires</summary>
        public DateTime? SubscriptionEndDate { get; set; }

        /// <summary>Whether this technician has an active subscription (paid or trial)</summary>
        [NotMapped]
        public bool IsSubscriptionActive =>
            Role == UserRole.Professional &&
            ((SubscriptionEndDate.HasValue && SubscriptionEndDate.Value > DateTime.UtcNow) ||
             (!SubscriptionEndDate.HasValue && RegistrationDate.AddDays(90) > DateTime.UtcNow));

        /// <summary>Whether still in the initial 3-month (90-day) free trial</summary>
        [NotMapped]
        public bool IsInTrialPeriod =>
            Role == UserRole.Professional &&
            !SubscriptionEndDate.HasValue &&
            RegistrationDate.AddDays(90) > DateTime.UtcNow;

        /// <summary>Trial end date (3 months from registration)</summary>
        [NotMapped]
        public DateTime TrialEndDate => RegistrationDate.AddDays(90);

        /// <summary>Days remaining in trial period</summary>
        [NotMapped]
        public int TrialDaysRemaining => Math.Max(0, (int)(TrialEndDate - DateTime.UtcNow).TotalDays);

        /// <summary>Whether subscription expired and technician is restricted</summary>
        [NotMapped]
        public bool IsInRestrictedMode =>
            Role == UserRole.Professional && !IsSubscriptionActive;

        public string GetFullName() => $"{FirstName} {LastName}";

        /// <summary>
        /// Returns the display username or falls back to full name
        /// </summary>
        public string GetDisplayName() => !string.IsNullOrEmpty(DisplayUsername) ? DisplayUsername : GetFullName();

        // ======== Mobile API Fields ========
        /// <summary>JWT Refresh Token for mobile authentication</summary>
        public string? RefreshToken { get; set; }

        /// <summary>Refresh token expiration date</summary>
        public DateTime? RefreshTokenExpiry { get; set; }

        /// <summary>Country code (ISO 3166-1 alpha-2)</summary>
        public string? Country { get; set; }

        /// <summary>Device token for push notifications (Firebase/APNs)</summary>
        public string? DevicePushToken { get; set; }

        /// <summary>Device platform (iOS, Android, Web)</summary>
        public string? DevicePlatform { get; set; }

        /// <summary>Whether user is verified (alias for IsProfileVerified for API)</summary>
        [NotMapped]
        public bool IsVerified => IsProfileVerified;
    }
}
