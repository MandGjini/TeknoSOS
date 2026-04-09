using System.ComponentModel.DataAnnotations;
using TeknoSOS.WebApp.Domain.Enums;

namespace TeknoSOS.WebApp.Domain.Entities
{
    /// <summary>
    /// Represents a business in the TeknoSOS ecosystem.
    /// Businesses can be material suppliers, service providers, or both.
    /// They operate within the closed marketplace and interact with technicians and clients.
    /// </summary>
    public class Business
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Slogan { get; set; }

        // Business Type and Category
        public BusinessType Type { get; set; } = BusinessType.Supplier;
        public BusinessCategory PrimaryCategory { get; set; }
        
        // Multiple categories (comma-separated category IDs)
        public string? AdditionalCategories { get; set; }

        // Contact Info (visible only to admins, masked from users)
        [StringLength(100)]
        public string? ContactEmail { get; set; }
        
        [StringLength(50)]
        public string? ContactPhone { get; set; }
        
        [StringLength(500)]
        public string? PhysicalAddress { get; set; }

        // Location (for map display)
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(100)]
        public string? Region { get; set; }
        
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? ServiceRadiusKm { get; set; }

        // Business Details
        [StringLength(50)]
        public string? NIPT { get; set; }  // Albanian business registration number
        
        [StringLength(100)]
        public string? LegalRepresentative { get; set; }
        
        public int? YearEstablished { get; set; }
        
        public int? EmployeeCount { get; set; }

        // Profile Media
        [StringLength(500)]
        public string? LogoUrl { get; set; }
        
        [StringLength(500)]
        public string? CoverImageUrl { get; set; }
        
        // Comma-separated list of gallery image URLs
        public string? GalleryImages { get; set; }

        // Operating Hours (JSON string for flexibility)
        public string? OperatingHours { get; set; }

        // Verification & Status
        public BusinessVerificationStatus VerificationStatus { get; set; } = BusinessVerificationStatus.Pending;
        public DateTime? VerificationDate { get; set; }
        
        [StringLength(500)]
        public string? VerificationNotes { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // Ratings & Stats
        public decimal AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public int CompletedOrders { get; set; } = 0;

        // Subscription & Trial (similar to technicians)
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsInTrialPeriod => SubscriptionEndDate.HasValue && 
                                       DateTime.UtcNow <= SubscriptionEndDate.Value &&
                                       SubscriptionStartDate.HasValue &&
                                       (DateTime.UtcNow - SubscriptionStartDate.Value).TotalDays <= 90;

        // Owner/Manager (linked to ApplicationUser)
        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }

        // Timestamps
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public ICollection<BusinessReview>? Reviews { get; set; }
        public ICollection<BusinessProduct>? Products { get; set; }

        // Helper properties
        public bool IsVerified => VerificationStatus == BusinessVerificationStatus.Verified;
        
        public string StatusBadgeClass => VerificationStatus switch
        {
            BusinessVerificationStatus.Verified => "bg-success",
            BusinessVerificationStatus.Pending => "bg-warning text-dark",
            BusinessVerificationStatus.Rejected => "bg-danger",
            BusinessVerificationStatus.Suspended => "bg-secondary",
            _ => "bg-secondary"
        };

        public string StatusText => VerificationStatus switch
        {
            BusinessVerificationStatus.Verified => "I Verifikuar",
            BusinessVerificationStatus.Pending => "Në Pritje",
            BusinessVerificationStatus.Rejected => "I Refuzuar",
            BusinessVerificationStatus.Suspended => "I Pezulluar",
            _ => "I Panjohur"
        };

        public string TypeText => Type switch
        {
            BusinessType.Supplier => "Furnizues Materialesh",
            BusinessType.ServiceProvider => "Ofrues Shërbimesh",
            BusinessType.Both => "Furnizues & Shërbime",
            _ => "Biznes"
        };

        public string CategoryText => PrimaryCategory switch
        {
            BusinessCategory.ElectricalSupplies => "Materiale Elektrike",
            BusinessCategory.PlumbingSupplies => "Materiale Hidraulike",
            BusinessCategory.ConstructionMaterials => "Materiale Ndërtimi",
            BusinessCategory.HVACSupplies => "Ngrohje/Ftohje",
            BusinessCategory.PaintAndFinishes => "Bojë & Përfundime",
            BusinessCategory.ToolsAndEquipment => "Vegla & Pajisje",
            BusinessCategory.SafetyEquipment => "Pajisje Sigurie",
            BusinessCategory.LightingSupplies => "Ndriçim",
            BusinessCategory.ArchitecturalServices => "Arkitekturë",
            BusinessCategory.EngineeringServices => "Inxhinieri",
            BusinessCategory.InteriorDesign => "Dizajn Enterieri",
            BusinessCategory.PermitServices => "Shërbime Lejesh",
            BusinessCategory.InspectionServices => "Inspektime",
            BusinessCategory.EquipmentRental => "Qiradhënie Pajisjesh",
            BusinessCategory.ScaffoldingRental => "Qiradhënie Skelash",
            BusinessCategory.VehicleRental => "Qiradhënie Automjetesh",
            BusinessCategory.WasteDisposal => "Largim Mbeturinash",
            BusinessCategory.Logistics => "Logjistikë",
            BusinessCategory.Cleaning => "Pastrim",
            BusinessCategory.Security => "Siguri",
            BusinessCategory.GeneralContractor => "Kontraktor",
            BusinessCategory.Other => "Të Tjera",
            _ => "Biznes"
        };

        public string MapMarkerColor => Type switch
        {
            BusinessType.Supplier => "#3b82f6",      // Blue
            BusinessType.ServiceProvider => "#8b5cf6", // Purple
            BusinessType.Both => "#06b6d4",          // Cyan
            _ => "#6b7280"                           // Gray
        };
    }

    /// <summary>
    /// Review for a business
    /// </summary>
    public class BusinessReview
    {
        public int Id { get; set; }
        
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        
        public string? ReviewerId { get; set; }
        public ApplicationUser? Reviewer { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsVerifiedPurchase { get; set; } = false;
    }

    /// <summary>
    /// Product or service offered by a business
    /// </summary>
    public class BusinessProduct
    {
        public int Id { get; set; }
        
        public int BusinessId { get; set; }
        public Business? Business { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public BusinessCategory Category { get; set; }
        
        public decimal? Price { get; set; }
        
        [StringLength(50)]
        public string? Unit { get; set; }  // copë, metër, kg, etc.
        
        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
