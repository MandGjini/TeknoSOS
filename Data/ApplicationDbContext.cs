using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeknoSOS.WebApp.Domain.Entities;
using TeknoSOS.WebApp.Models;

namespace TeknoSOS.WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        
        public DbSet<Review> Reviews { get; set; }
        
        public DbSet<ProfessionalSpecialty> ProfessionalSpecialties { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<TechnicianInterest> TechnicianInterests { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<SiteContent> SiteContents { get; set; }

        public DbSet<SiteSetting> SiteSettings { get; set; }

        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }

        public DbSet<SiteMenu> SiteMenus { get; set; }

        public DbSet<TechnicianCertificate> TechnicianCertificates { get; set; }

        public DbSet<TechnicianPortfolio> TechnicianPortfolios { get; set; }

        public DbSet<TechnicianSubscription> TechnicianSubscriptions { get; set; }

        public DbSet<Banner> Banners { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<TeknoSOS.WebApp.Models.KarikuesEVPageContent> KarikuesEVPageContents { get; set; }

        // Business ecosystem entities
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessReview> BusinessReviews { get; set; }
        public DbSet<BusinessProduct> BusinessProducts { get; set; }

        // Partner & Blog entities
        public DbSet<Partner> Partners { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<TeknoSOS.WebApp.Domain.Entities.QuickCase> QuickCases { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ServiceRequest relationships
            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Citizen)
                .WithMany(u => u.CreatedServiceRequests)
                .HasForeignKey(sr => sr.CitizenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Professional)
                .WithMany(u => u.AssignedServiceRequests)
                .HasForeignKey(sr => sr.ProfessionalId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Review)
                .WithOne(r => r.ServiceRequest)
                .HasForeignKey<Review>(r => r.ServiceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review relationships
            builder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.WrittenReviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReceivedReviews)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.NoAction);

            // ProfessionalSpecialty relationships
            builder.Entity<ProfessionalSpecialty>()
                .HasOne(ps => ps.Professional)
                .WithMany(u => u.Specialties)
                .HasForeignKey(ps => ps.ProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification relationships
            builder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.ServiceRequest)
                .WithMany(sr => sr.Notifications)
                .HasForeignKey(n => n.ServiceRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // Message relationships
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Message>()
                .HasOne(m => m.ServiceRequest)
                .WithMany(sr => sr.Messages)
                .HasForeignKey(m => m.ServiceRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // TechnicianInterest relationships
            builder.Entity<TechnicianInterest>()
                .HasOne(ti => ti.Technician)
                .WithMany(u => u.Interests)
                .HasForeignKey(ti => ti.TechnicianId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TechnicianInterest>()
                .HasOne(ti => ti.ServiceRequest)
                .WithMany(sr => sr.TechnicianInterests)
                .HasForeignKey(ti => ti.ServiceRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            // AuditLog relationships
            builder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal precision
            builder.Entity<ServiceRequest>()
                .Property(sr => sr.EstimatedCost)
                .HasPrecision(10, 2);

            builder.Entity<ServiceRequest>()
                .Property(sr => sr.FinalCost)
                .HasPrecision(10, 2);

            builder.Entity<ServiceRequest>()
                .Property(sr => sr.ClientLatitude)
                .HasPrecision(10, 8);

            builder.Entity<ServiceRequest>()
                .Property(sr => sr.ClientLongitude)
                .HasPrecision(10, 8);

            builder.Entity<ServiceRequest>()
                .Property(sr => sr.NearestTechnicianDistance)
                .HasPrecision(10, 2);

            builder.Entity<ProfessionalSpecialty>()
                .Property(ps => ps.HourlyRate)
                .HasPrecision(10, 2);

            builder.Entity<TechnicianInterest>()
                .Property(ti => ti.EstimatedCost)
                .HasPrecision(10, 2);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Latitude)
                .HasPrecision(10, 8);

            // Global filter: hide users marked as inactive (deleted/anonymized)
            // This makes `DbContext.Users` and related queries exclude those records
            // while keeping active Admins, Professionals and Citizens available.
            builder.Entity<ApplicationUser>()
                .HasQueryFilter(u => u.IsActive);

            builder.Entity<ApplicationUser>()
                .Property(u => u.Longitude)
                .HasPrecision(10, 8);

            builder.Entity<ApplicationUser>()
                .Property(u => u.AverageRating)
                .HasPrecision(3, 2);

            // SiteContent index for fast lookups
            builder.Entity<SiteContent>()
                .HasIndex(sc => new { sc.PageKey, sc.SectionKey, sc.Language })
                .HasDatabaseName("IX_SiteContent_Page_Section_Lang");

            builder.Entity<SiteContent>()
                .HasIndex(sc => new { sc.PageKey, sc.Language })
                .HasDatabaseName("IX_SiteContent_Page_Lang");

            // SiteSetting unique constraint
            builder.Entity<SiteSetting>()
                .HasIndex(ss => new { ss.GroupKey, ss.SettingKey })
                .IsUnique()
                .HasDatabaseName("IX_SiteSetting_Group_Key");

            // SiteMenu self-referencing hierarchy
            builder.Entity<SiteMenu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SiteMenu>()
                .HasIndex(m => new { m.MenuLocation, m.SortOrder })
                .HasDatabaseName("IX_SiteMenu_Location_Sort");

            // ServiceRequest UniqueCode unique index
            builder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.UniqueCode)
                .IsUnique()
                .HasDatabaseName("IX_ServiceRequest_UniqueCode");

            // TechnicianCertificate relationships
            builder.Entity<TechnicianCertificate>()
                .HasOne(tc => tc.Technician)
                .WithMany(u => u.Certificates)
                .HasForeignKey(tc => tc.TechnicianId)
                .OnDelete(DeleteBehavior.Cascade);

            // TechnicianPortfolio relationships
            builder.Entity<TechnicianPortfolio>()
                .HasOne(tp => tp.Technician)
                .WithMany(u => u.PortfolioItems)
                .HasForeignKey(tp => tp.TechnicianId)
                .OnDelete(DeleteBehavior.Cascade);

            // TechnicianSubscription relationships
            builder.Entity<TechnicianSubscription>()
                .HasOne(ts => ts.Technician)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(ts => ts.TechnicianId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TechnicianSubscription>()
                .Property(ts => ts.Amount).HasPrecision(10, 2);

            // Invoice relationships
            builder.Entity<Invoice>()
                .HasOne(i => i.Technician)
                .WithMany()
                .HasForeignKey(i => i.TechnicianId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Invoice>()
                .Property(i => i.Amount).HasPrecision(10, 2);
            builder.Entity<Invoice>()
                .Property(i => i.TaxAmount).HasPrecision(10, 2);
            builder.Entity<Invoice>()
                .Property(i => i.TotalAmount).HasPrecision(10, 2);

            builder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique()
                .HasDatabaseName("IX_Invoice_InvoiceNumber");

            // Business entity configuration
            builder.Entity<Business>()
                .HasOne(b => b.Owner)
                .WithMany()
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Business>()
                .Property(b => b.Latitude)
                .HasPrecision(10, 8);

            builder.Entity<Business>()
                .Property(b => b.Longitude)
                .HasPrecision(10, 8);

            builder.Entity<Business>()
                .Property(b => b.AverageRating)
                .HasPrecision(3, 2);

            builder.Entity<Business>()
                .HasIndex(b => b.NIPT)
                .HasDatabaseName("IX_Business_NIPT");

            builder.Entity<Business>()
                .HasIndex(b => new { b.IsActive, b.VerificationStatus })
                .HasDatabaseName("IX_Business_Active_Status");

            // BusinessReview relationships
            builder.Entity<BusinessReview>()
                .HasOne(br => br.Business)
                .WithMany(b => b.Reviews)
                .HasForeignKey(br => br.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BusinessReview>()
                .HasOne(br => br.Reviewer)
                .WithMany()
                .HasForeignKey(br => br.ReviewerId)
                .OnDelete(DeleteBehavior.SetNull);

            // BusinessProduct relationships
            builder.Entity<BusinessProduct>()
                .HasOne(bp => bp.Business)
                .WithMany(b => b.Products)
                .HasForeignKey(bp => bp.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BusinessProduct>()
                .Property(bp => bp.Price)
                .HasPrecision(10, 2);

            builder.Entity<BusinessProduct>()
                .HasIndex(bp => new { bp.BusinessId, bp.IsAvailable })
                .HasDatabaseName("IX_BusinessProduct_Business_Available");

            // BlogPost unique slug index
            builder.Entity<BlogPost>()
                .HasIndex(bp => bp.Slug)
                .IsUnique()
                .HasDatabaseName("IX_BlogPost_Slug");

            // QuickCase index for recent lookups
            builder.Entity<TeknoSOS.WebApp.Domain.Entities.QuickCase>()
                .HasIndex(q => q.CreatedDate)
                .HasDatabaseName("IX_QuickCase_CreatedDate");

            // ===== Performance Indexes for Common Queries =====
            
            // ServiceRequest indexes for citizen/professional lookups and status filtering
            builder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.CitizenId)
                .HasDatabaseName("IX_ServiceRequest_CitizenId");

            builder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.ProfessionalId)
                .HasDatabaseName("IX_ServiceRequest_ProfessionalId");

            builder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.Status)
                .HasDatabaseName("IX_ServiceRequest_Status");

            builder.Entity<ServiceRequest>()
                .HasIndex(sr => sr.CreatedDate)
                .HasDatabaseName("IX_ServiceRequest_CreatedDate");

            // Message indexes for chat queries
            builder.Entity<Message>()
                .HasIndex(m => m.SenderId)
                .HasDatabaseName("IX_Message_SenderId");

            builder.Entity<Message>()
                .HasIndex(m => m.ReceiverId)
                .HasDatabaseName("IX_Message_ReceiverId");

            builder.Entity<Message>()
                .HasIndex(m => m.ServiceRequestId)
                .HasDatabaseName("IX_Message_ServiceRequestId");

            builder.Entity<Message>()
                .HasIndex(m => m.CreatedDate)
                .HasDatabaseName("IX_Message_CreatedDate");

            // Notification indexes for efficient unread notification queries
            builder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientId, n.IsRead })
                .HasDatabaseName("IX_Notification_RecipientId_IsRead");

            builder.Entity<Notification>()
                .HasIndex(n => n.CreatedDate)
                .HasDatabaseName("IX_Notification_CreatedDate");

            // TechnicianInterest indexes
            builder.Entity<TechnicianInterest>()
                .HasIndex(ti => ti.ServiceRequestId)
                .HasDatabaseName("IX_TechnicianInterest_ServiceRequestId");

            builder.Entity<TechnicianInterest>()
                .HasIndex(ti => ti.TechnicianId)
                .HasDatabaseName("IX_TechnicianInterest_TechnicianId");

            // Review indexes for rating queries
            builder.Entity<Review>()
                .HasIndex(r => r.ReviewerId)
                .HasDatabaseName("IX_Review_ReviewerId");

            builder.Entity<Review>()
                .HasIndex(r => r.RevieweeId)
                .HasDatabaseName("IX_Review_RevieweeId");

            // AuditLog index for user activity queries
            builder.Entity<AuditLog>()
                .HasIndex(al => al.UserId)
                .HasDatabaseName("IX_AuditLog_UserId");

            builder.Entity<AuditLog>()
                .HasIndex(al => al.CreatedDate)
                .HasDatabaseName("IX_AuditLog_CreatedDate");

            // Invoice index for technician billing queries
            builder.Entity<Invoice>()
                .HasIndex(i => i.TechnicianId)
                .HasDatabaseName("IX_Invoice_TechnicianId");

            builder.Entity<Invoice>()
                .HasIndex(i => i.Status)
                .HasDatabaseName("IX_Invoice_Status");

            // TechnicianSubscription index
            builder.Entity<TechnicianSubscription>()
                .HasIndex(ts => ts.TechnicianId)
                .HasDatabaseName("IX_TechnicianSubscription_TechnicianId");

            // Banner index for active banner queries
            builder.Entity<Banner>()
                .HasIndex(b => new { b.IsActive, b.StartDate, b.EndDate })
                .HasDatabaseName("IX_Banner_Active_DateRange");

            // ProfessionalSpecialty index for specialty lookups
            builder.Entity<ProfessionalSpecialty>()
                .HasIndex(ps => ps.ProfessionalId)
                .HasDatabaseName("IX_ProfessionalSpecialty_ProfessionalId");

            builder.Entity<ProfessionalSpecialty>()
                .HasIndex(ps => ps.Category)
                .HasDatabaseName("IX_ProfessionalSpecialty_Category");
        }
    }
}
